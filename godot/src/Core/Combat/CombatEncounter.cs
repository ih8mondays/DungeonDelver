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
    private readonly IReadOnlyList<Creature> _monsters;
    private readonly int _maxRounds;
    private CombatState State { get; set; } = CombatState.InProgress;
    private bool IsCombatFinished => State != CombatState.InProgress;
    private int Round { get; set; } = 1;

    public CombatEncounter(Player player, IReadOnlyList<Creature> monsters, int maxRounds = 100) {
        ArgumentNullException.ThrowIfNull(player);
        ArgumentNullException.ThrowIfNull(monsters);
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(maxRounds);
        
        var copy = monsters.ToArray();                       // snapshot
        if (copy.Length is < 1 or > 3)
            throw new ArgumentOutOfRangeException(nameof(monsters), copy.Length, "Combat needs 1 to 3 monsters.");

        for (var i = 0; i < copy.Length; i++) {
            if (copy[i] is null)
                throw new ArgumentException($"Monster at index {i} is null.", nameof(monsters));
            if (!copy[i].IsAlive)
                throw new ArgumentException($"Monster '{copy[i].Name}' is already dead.", nameof(monsters));
        }

        _player = player;
        _monsters = Array.AsReadOnly(copy);
        _maxRounds = maxRounds;
    }

    //public StepResult Step() {
    //    if (IsCombatFinished)
    //        throw new InvalidOperationException("Combat is over.");
    //    
    //    var actor  = NextLivingActor();      // skip dead monsters, advance _cursor, bump Round on wrap
    //    var target = PickTarget(actor);      // player -> first living monster, monster -> player
    //    var ability = actor.Abilities[0];
    //    
    //    var calculated = CombatCalculator.CalculateDamage(ability, actor.AttackPower);
    //    var dealt = target.Damage(calculated);   // public API only
    //    var targetDied = false;
    //    Round++;
    //    State = CombatState.InProgress; //EvaluateState();
    //    return new StepResult(actor, target, ability.Name, calculated, dealt, targetDied, Round,State);
    //}
    
}
