using System;
using System.Collections.Generic;
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
    private int _cursor;   // 0 = player, 1..n = monsters

    public CombatState State { get; private set; } = CombatState.InProgress;
    public bool IsCombatFinished => State != CombatState.InProgress;
    public int Round { get; private set; } = 1;

    public CombatEncounter(Player player, IReadOnlyList<Creature> monsters, int maxRounds = 100) {
        // guard clauses: nulls, 1..3 monsters, maxRounds > 0, everyone alive, everyone has an ability
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
    //
    //    var roundOfStep = /* the round this action happened in, not the one after the cursor moved */;
    //    State = EvaluateState();
    //    return new StepResult(/* ... */);
    //}
    
}
