using DungeonDelver.Model.Creatures.CreatureDto;

namespace DungeonDelver.Model.Creatures;

public class Player(string name, int maxHp, int attackPower) : Creature(name, maxHp, attackPower) { 
    
    // For JSON purposes ->
    // var data = JsonSerializer.Deserialize<CreatureData>(json)!;
    // var goblin = new Goblin(data);
    public Player(PlayerDto data) : this(data.Name, data.MaxHp, data.AttackPower) { }

}