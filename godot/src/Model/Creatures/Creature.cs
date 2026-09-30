using System;
using System.Collections.Generic;
using DungeonDelver.Model.Abilities;

namespace DungeonDelver.Model.Creatures;

public abstract class Creature {
	
    public string Name { get; }
    public int MaxHP { get; }
    public int CurrentHP { get; private set; }
    public bool IsAlive { get; private set; }
    public int AttackPower { get; }
    private readonly List<Ability> _abilities = new();
	public IReadOnlyList<Ability> Abilities => _abilities;
	private readonly List<StatusEffect> _statsEffects = new();


	public Creature(string name,  int maxHP, int attackPower, int? currentHP = null) {
		
		if (String.IsNullOrWhiteSpace(name)) { throw new ArgumentException("Name cannot be empty"); }
		if (maxHP <= 0) { throw new ArgumentOutOfRangeException("MaxHP can't be negative or zero"); }
		if (currentHP <= 0) { throw new ArgumentOutOfRangeException("CurrentHP can't be negative or zero"); }
		if (attackPower < 0) { throw new ArgumentOutOfRangeException("AttackPower can't be negative"); }

		Name = name;
		MaxHP = maxHP;
		CurrentHP = currentHP ?? MaxHP;
		IsAlive = true;
		AttackPower = attackPower;
	}
	
    
    public void Damage(int damage) {

	    if (damage < 0) { throw new ArgumentOutOfRangeException("Damage can't be negative. Target: " + Name); }
	    
	    if (damage >= CurrentHP) {
		    CurrentHP = 0;
		    IsAlive = false;
	    }
	    else {
		    CurrentHP -= damage;
	    }
    }

    public void Heal(int healing) {
	    if (!IsAlive) { return; } //TODO Add an exception
	    
	    if ( healing < 0) { throw new ArgumentOutOfRangeException("Healing can't be negative"); }

	    if (CurrentHP + healing > MaxHP) {
		    CurrentHP = MaxHP; 
	    } else {
		    CurrentHP += healing;
	    }
    }

    public void AddAbility(Ability ability) {
	    
	    ArgumentNullException.ThrowIfNull(ability);
	    
	    if (_abilities.Contains(ability)) { 
		    throw new Exception("Ability already exists on creature: " + Name);
	    }
	    
        _abilities.Add(ability);
    }

    public void RemoveAbility(Ability ability) {
	    _abilities.Remove(ability);
    }
    
}