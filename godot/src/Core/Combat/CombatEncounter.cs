using System;
using System.Collections.Generic;
using System.Linq;
using DungeonDelver.Model.Creatures;

namespace DungeonDelver.Core.Combat;

public enum CombatState { InProgress, Victory, Defeat, Stalemate }

public sealed record StepResult(
    string Actor, string Target, string AbilityName,
    int Calculated, int Dealt, bool TargetDied,
    int Round, CombatState StateAfter);

public class CombatEncounter {
    private readonly Player _player;
    private readonly List<Creature> _monsters;
    private readonly List<Creature> _combatants;
    private int _cursor = 0;    // effectively a current combatant "pointer"
    private readonly int _maxRounds;
    private int _round;
    private CombatState _state;
    private bool IsCombatFinished => _state != CombatState.InProgress;
    

    public CombatEncounter(Player player, List<Creature> monsters, int maxRounds = 100) {
        ArgumentNullException.ThrowIfNull(player);
        if (!player.IsAlive) throw new CombatEncounterException("You have died, game over.");
        ArgumentNullException.ThrowIfNull(monsters);
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(maxRounds);
        
        if (monsters.Count is < 1 or > 3)
            throw new ArgumentOutOfRangeException(nameof(monsters), monsters.Count, "Combat needs 1 to 3 monsters.");
        
        foreach (var creature in monsters) {
            ArgumentNullException.ThrowIfNull(creature);
            if (!creature.IsAlive)
                throw new ArgumentException($"Monster '{creature.Name}' is already dead.", nameof(monsters));
        }
        
        _combatants = [player, .. monsters]; // must be a shallow copy
        _player = player;
        _monsters = [.. monsters];
        _maxRounds = maxRounds;
        _round = 0;
        _cursor = 0;
        _state = CombatState.InProgress;
    }

    public StepResult Step() {
        if (IsCombatFinished)
            throw new InvalidOperationException("Combat is over.");
        
        var actor  = _combatants[_cursor];
        var target = ChooseTarget(actor);
        var ability = actor.Abilities[0];
        
        var calculated = CombatCalculator.CalculateDamage(ability, actor.AttackPower);
        var dealt = target.Damage(calculated);
        var targetDied = !target.IsAlive;
        
        EvaluateState();
        if (!IsCombatFinished) // Avoids possible infinite loops
            MoveCursor();
        
        //TODO
        // 1. The constructor still takes List<Creature> and doesn't reject duplicates.
        // 2. StepResult identifies creatures by name.
        return new StepResult(actor.Name, target.Name, ability.Name, calculated, dealt, targetDied, _round, _state);
    }

    private Creature ChooseTarget(Creature actor) {
        // player attacks closest living monster.
        // monster attacks player
        return actor == _player
            ? _monsters.First(x => x.IsAlive)
            : _player;
    }

    private void EvaluateState() {
         if (!_player.IsAlive) {
            _state = CombatState.Defeat;
         } else if (!_monsters.Any(x => x.IsAlive)) {
            _state = CombatState.Victory;
         } else if (_round > _maxRounds) {
            _state = CombatState.Stalemate;
         }
    }
    
    private void MoveCursor() {
        // skip dead monsters, advance _cursor
        do {
            if (_cursor == 0)
                _round++;
            _cursor = (_cursor + 1) % _combatants.Count;
        } while (!_combatants[_cursor].IsAlive);
    }
    
}
