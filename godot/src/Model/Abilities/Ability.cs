using System;
using System.Collections.Generic;

namespace DungeonDelver.Model.Abilities;

public class Ability {
    public string Name { get; }
    public int BaseDamage { get; }
    public int AttackPowerScalingPercent { get; }
    public int ProcChancePercent {get;}
    private readonly List<StatusEffect> _appliedStatusEffects;
    public IReadOnlyList<StatusEffect> AppliedStatusEffects => _appliedStatusEffects.AsReadOnly();
    public string IconId { get; }
    public string SpriteId { get; }

    /// <summary>
    /// TODO
    /// </summary>
    /// <param name="name">Must be unique</param>
    /// <param name="baseDamage">Can't be negative.</param>
    /// <param name="attackPowerScalingPercent">0 to 100</param>
    /// <param name="procChancePercent">0 to 100</param>
    /// <param name="iconId"></param>
    /// <param name="spriteId"></param>
    /// <param name="statusEffects">List of Status Effects to apply</param>
    /// <exception cref="ArgumentException">Throws on missing string arguments</exception>
    /// <exception cref="ArgumentOutOfRangeException">Throws if values are out of specified range</exception>
    public Ability(string name, int baseDamage, int attackPowerScalingPercent, int procChancePercent, string iconId, string spriteId, List<StatusEffect> statusEffects = null) {
        ArgumentException.ThrowIfNullOrWhiteSpace(name);
        ArgumentOutOfRangeException.ThrowIfNegative(baseDamage);
        if (attackPowerScalingPercent < 0 || attackPowerScalingPercent > 100) throw new ArgumentOutOfRangeException(nameof(attackPowerScalingPercent));
        if (procChancePercent < 0 || procChancePercent > 100) throw new ArgumentOutOfRangeException(nameof(procChancePercent));
        if (string.IsNullOrWhiteSpace(iconId)) throw new ArgumentException("IconId cannot be empty");
        if (string.IsNullOrWhiteSpace(spriteId)) throw new ArgumentException("SpriteId cannot be empty");
        
        Name = name;
        BaseDamage = baseDamage;
        AttackPowerScalingPercent = attackPowerScalingPercent;
        ProcChancePercent = procChancePercent;
        _appliedStatusEffects = statusEffects ?? [];
        IconId = iconId;
        SpriteId = spriteId;
    }
    
}
