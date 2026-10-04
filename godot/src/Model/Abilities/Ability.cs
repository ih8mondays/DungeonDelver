using System;
using System.Collections.Generic;
using DungeonDelver.Model.Creatures;

namespace DungeonDelver.Model.Abilities;

public abstract class Ability {
    
    public string Name { get; }
    public int BaseDamage { get; }
    public float AttackPowerScaling { get; }
    protected float ProcChance;
    protected List<StatusEffect> AppliedStatusEffects;
    protected string IconId;
    protected string SpriteId;

    protected Ability(string name, int baseDamage, float attackPowerScaling, float procChance, string iconId, string spriteId, List<StatusEffect> statusEffects = null) {
        ArgumentException.ThrowIfNullOrWhiteSpace(name);
        ArgumentOutOfRangeException.ThrowIfNegative(baseDamage);
        if (attackPowerScaling < 0 || attackPowerScaling > 1) throw new ArgumentOutOfRangeException(nameof(attackPowerScaling));
        if (procChance < 0 || procChance > 1) throw new ArgumentOutOfRangeException(nameof(procChance));
        if (string.IsNullOrWhiteSpace(iconId)) throw new ArgumentException("IconId cannot be empty");
        if (string.IsNullOrWhiteSpace(spriteId)) throw new ArgumentException("SpriteId cannot be empty");
        
        Name = name;
        BaseDamage = baseDamage;
        AttackPowerScaling = attackPowerScaling;
        ProcChance = procChance;
        AppliedStatusEffects = statusEffects ?? [];
        IconId = iconId;
        SpriteId = spriteId;
    }
    
    public abstract void Use(Creature user, Creature target);
    
}
