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
    private int _cursor = 0;
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
        
        // shallow copy. don't touch or stuff breaks.
        _combatants = [player, .. monsters];
        _player = player;
        _monsters = [.. monsters];
        _maxRounds = maxRounds;
        _round = 1;
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
        MoveCursor();
        
        return new StepResult(actor.Name, target.Name, ability.Name, calculated, dealt, targetDied, _round, _state);
    }

    private Creature ChooseTarget(Creature actor) {
        // player -> first living monster.
        // monster -> player
        return actor is Player 
            ? _monsters.First(x => x.IsAlive)
            : _player;
    }

    private void EvaluateState() {
        if (_round >= _maxRounds) {
            _state = CombatState.Stalemate;
        } else if (!_player.IsAlive) {
            _state = CombatState.Defeat;
        } else if (!_monsters.Any(x => x.IsAlive)) {
            _state = CombatState.Victory;
        }
    }
    
    private void MoveCursor() {
        // skip dead monsters, advance _cursor
        do {
            if (_cursor == _combatants.Count - 1) 
                _round++;
            _cursor = (_cursor + 1) % _combatants.Count;
        } while (!_combatants[_cursor].IsAlive);
    }
    
}
