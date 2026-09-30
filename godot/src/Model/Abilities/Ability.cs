using System;

namespace DungeonDelver.Model.Abilities;

public abstract class Ability {
    
    public string Name;
    public int BaseDamage;
    public string IconId;
    public string SpriteId;

    public Ability(string name, int baseDamage, string iconId, string spriteId) {
        if (String.IsNullOrWhiteSpace(name)) { throw new ArgumentException("Name cannot be empty"); }
        if (baseDamage <= 0) { throw new ArgumentOutOfRangeException("BaseDamage can't be negative or zero"); }
        if  (String.IsNullOrWhiteSpace(iconId)) { throw new ArgumentException("IconId cannot be empty"); }
        if (String.IsNullOrWhiteSpace(spriteId)) { throw new ArgumentException("SpriteId cannot be empty"); }
        
        Name = name;
        BaseDamage = baseDamage;
        IconId = iconId;
        SpriteId = spriteId;
    }
    
    public abstract void Use();

    // Used in Creature.AddAbility() -> List.Contains()
    // We don't want duplicate abilities... for now
    public bool Equals(Ability other) {
        return true;
    }
    
}