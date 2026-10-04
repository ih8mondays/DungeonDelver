using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using DungeonDelver.Model.Abilities;

namespace DungeonDelver.Model.Creatures;

public readonly record struct HpChangedEventArgs(int OldHp, int NewHp, int MaxHp);

public class Creature {
	
    public string Name { get; }
    public int MaxHp { get; }
    public int CurrentHp { get; private set; }
    public bool IsAlive => CurrentHp > 0;
    public int AttackPower { get; }
    private readonly List<Ability> _abilities = new();
	public IReadOnlyList<Ability> Abilities => _abilities.AsReadOnly();
	private readonly List<StatusEffect> _statusEffects = new();
	public IReadOnlyList<StatusEffect> StatusEffects => _statusEffects.AsReadOnly();
	public event EventHandler<HpChangedEventArgs>? HpChanged;
	public event EventHandler? Died;

	
	public Creature(string name,  int maxHp, int attackPower, int? currentHp = null) {
		
		ArgumentException.ThrowIfNullOrWhiteSpace(name);
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(maxHp);
        ArgumentOutOfRangeException.ThrowIfNegative(attackPower);
        
        if (currentHp is not null) {
	        var hp = currentHp.Value;
	        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(hp);
	        ArgumentOutOfRangeException.ThrowIfGreaterThan(hp, maxHp);
        }
        
		Name = name;
		MaxHp = maxHp;
		CurrentHp = currentHp ?? MaxHp;
		AttackPower = attackPower;
	}
	
    /// <summary>
    /// Deals damage to target based on input.
    /// Dead creatures can't be damaged and returns 0.
    /// </summary>
    /// <param name="amount"></param>
    /// <returns>The actual damage dealt.</returns>
    /// <exception cref="ArgumentOutOfRangeException">Throws on negative input.</exception>
    public int Damage(int amount) {
	    ArgumentOutOfRangeException.ThrowIfNegative(amount);
	    var newHp = Math.Max(0, CurrentHp - amount);
	    return -SetHp(newHp);
    }

    /// <summary>
    /// Heals target based on input.
	/// Dead creatures can't be healed and returns 0.
    /// </summary>
    /// <param name="amount"></param>
    /// <returns>The actual healing done.</returns>
    /// <exception cref="ArgumentOutOfRangeException">Throws on negative input.</exception>
    public int Heal(int amount) {
	    ArgumentOutOfRangeException.ThrowIfNegative(amount);
	    // overflow protection
	    var maxHeal = MaxHp - CurrentHp;
	    var newHp = CurrentHp + Math.Min(amount, maxHeal);
	    return SetHp(newHp);
    }
    
    /// <returns>The signed HP change (new - old). 0 if nothing changed.</returns>
    private int SetHp(int newHp) {
	    Debug.Assert(newHp >= 0 && newHp <= MaxHp);
	    
	    var oldHp = CurrentHp;
	    if (!IsAlive || newHp == oldHp) return 0;
	    CurrentHp = newHp;
	    HpChanged?.Invoke(this, new HpChangedEventArgs(oldHp, newHp, MaxHp));
	    if (newHp == 0) Died?.Invoke(this, EventArgs.Empty);
	    return newHp - oldHp;
    }
	
    public void AddAbility(Ability ability) {
	    ArgumentNullException.ThrowIfNull(ability);
	    if (_abilities.Any(a => a.Name == ability.Name)) { 
		    throw new InvalidOperationException("Creature already knows this ability: " + ability.Name);
	    }
	    
        _abilities.Add(ability);
    }
    
    public bool RemoveAbility(Ability ability) {
	    return _abilities.RemoveAll(a => a.Name == ability.Name) > 0;
    }
    
    /// <exception cref="ArgumentNullException"></exception>
    /// <exception cref="InvalidOperationException">Throws if ability already exists on the creature</exception>
    public void AddStatusEffect(StatusEffect statusEffect) {
	    ArgumentNullException.ThrowIfNull(statusEffect);
	    if (_statusEffects.Any(s => s.Name == statusEffect.Name)) { 
		    throw new InvalidOperationException("Creature already knows this ability: " + statusEffect.Name);
	    }
	    
	    _statusEffects.Add(statusEffect);
    }
    
    /// <returns>Returns true if removal was successful.</returns>
    public bool RemoveStatusEffect(StatusEffect statusEffect) {
	    return _abilities.RemoveAll(s => s.Name == statusEffect.Name) > 0;
    }
    
}
