using System;
using System.Collections.Generic;
using DungeonDelver.Model.Creatures;

namespace DungeonDelver.Model.Abilities;

public abstract class Ability {
    
    protected string Name;
    protected int BaseDamage;
    protected float AttackPowerScaling;
    protected float ProcChance;
    protected List<StatusEffect> AppliedStatusEffects;
    protected string IconId;
    protected string SpriteId;

    protected Ability(string name, int baseDamage, float attackPowerScaling, float procChance, string iconId, string spriteId, List<StatusEffect> statusEffects = null) {
        if (string.IsNullOrWhiteSpace(name)) { throw new ArgumentException("Name cannot be empty"); }
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(baseDamage);
        if (attackPowerScaling < 0 || attackPowerScaling > 1) { throw new ArgumentOutOfRangeException("AttackPowerScaling must be between 0 and 1"); }
        if (procChance < 0 || procChance > 1) { throw new ArgumentOutOfRangeException("ProcChance must be between 0 and 1"); }
        if (string.IsNullOrWhiteSpace(iconId)) { throw new ArgumentException("IconId cannot be empty"); }
        if (string.IsNullOrWhiteSpace(spriteId)) { throw new ArgumentException("SpriteId cannot be empty"); }
        
        Name = name;
        BaseDamage = baseDamage;
        AttackPowerScaling = attackPowerScaling;
        ProcChance = procChance;
        AppliedStatusEffects = statusEffects ?? [];
        IconId = iconId;
        SpriteId = spriteId;
    }
    
    public abstract void Use(Creature user, Creature target);

    public int CalculateDamage(Creature user) {
        return (int)(BaseDamage + user.AttackPower * AttackPowerScaling);
    }

    // Used in Creature.AddAbility() -> List.Contains()
    // We don't want duplicate abilities... for now
    public bool Equals(Ability other) {
        return true;
    }
    
}
