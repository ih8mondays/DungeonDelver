using System;
using DungeonDelver.Model.Abilities;

namespace DungeonDelver.Core.Combat;

public class CombatCalculator {
    
    /// <summary>
    /// Calculates damage output to be fed into Creature.Damage()
    /// </summary>
    /// <param name="ability">The ability being used.</param>
    /// <param name="attackPower">The attacking creatures AttackPower</param>
    /// <returns>Damage to be dealt after calculations.</returns>
    /// <exception cref="ArgumentNullException">Throws if ability provided is null.</exception>
    public static int CalculateDamage(Ability ability, int attackPower) {
        ArgumentNullException.ThrowIfNull(ability);
        
        // Overflow protection might be overkill, but better safe than sorry.
        var raw = ability.BaseDamage + (long)attackPower * ability.AttackPowerScalingPercent / 100;
        return (int)Math.Clamp(raw, 0, int.MaxValue);
    }
}
