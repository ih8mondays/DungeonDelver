using System.Collections.Generic;
using DungeonDelver.Model.Abilities;

namespace DungeonDelver.Model.Creatures;

// spriteId = key for the picture (e.g. "goblin1"). the Scenes layer turns it into a texture,
// so the model never touches Godot resources. no sprite given -> DefaultSpriteId.
public abstract class Creature(string name, int maxHP, int attackPower, string spriteId = null, int? currentHP = null) {
	
    public const string DefaultSpriteId = "default";
	
    public string Name { get; } = name;
    public int MaxHP { get; } = maxHP;
    public int CurrentHP { get; private set; } = currentHP ?? maxHP;
    public bool IsAlive = true;
    public int AttackPower { get; } = attackPower;
    public string SpriteId { get; } = string.IsNullOrWhiteSpace(spriteId) ? DefaultSpriteId : spriteId;
	private readonly List<Ability> _abilities = new();
	public IReadOnlyList<Ability> Abilities => _abilities;
	
	/// adds an ability to this creature
	public void Learn(Ability ability) => _abilities.Add(ability);
	
    public void Attack() {
        
    }
    
    public void TakeDamage(int damage) {
	    
	    if (damage > CurrentHP) {
		    CurrentHP = 0;
		    IsAlive = false;
	    }
	    else {
		    CurrentHP -= damage;
	    }
    }
    
    
}