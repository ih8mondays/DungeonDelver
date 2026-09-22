using System.Collections.Generic;
using DungeonDelver.Model.Abilities;

namespace DungeonDelver.Model.Creatures;

public abstract class Creature(string name,  int maxHP, int attackPower, int? currentHP = null) {
	
    public required string Name { get; init; } = name;
    public required int MaxHP { get; init; } = maxHP;
    public int CurrentHP { get; private set; } = currentHP ?? maxHP;
    public bool IsAlive = true;
    public required int AttackPower { get; init; } = attackPower;
	private readonly List<Ability> _abilities = new();
	public IReadOnlyList<Ability> Abilities => _abilities;
	
	
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