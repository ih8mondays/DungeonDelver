using System.Collections.Generic;
using DungeonDelver.Model.Abilities;

namespace DungeonDelver.Model.Creatures;

public abstract class Creature {
    public required string Name { get; init; }
    public required int MaxHP { get; init; }
    public int CurrentHP { get; private set; }
    public bool IsAlive => CurrentHP > 0;
	public required int AttackPower { get; init; }
	private readonly List<Ability> _abilities;
	public IReadOnlyList<Ability> Abilities => _abilities;
	
	
	public Creature(string name,  int maxHP, int attackPower) {
		Name = name;
		MaxHP = maxHP;
		CurrentHP = maxHP;
		AttackPower = attackPower;
		_abilities = new();
	}
	
	// For JSON purposes ->
	// var data = JsonSerializer.Deserialize<CreatureData>(json)!;
	// var goblin = new Goblin(data);
	public Creature(CreatureData data) {
		Name = data.Name;
		MaxHP = data.MaxHP;
		CurrentHP = data.MaxHP;
		AttackPower = data.AttackPower;
		_abilities = new();
	}
	
    public void Attack() {
        
    }
    
    
}