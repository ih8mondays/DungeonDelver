using DungeonDelver.Model.Creatures.CreatureData;

namespace DungeonDelver.Model.Creatures;

public class Player(string name, int maxHP, int attackPower) : Creature(name, maxHP, attackPower) { 
    
    // For JSON purposes ->
    // var data = JsonSerializer.Deserialize<CreatureData>(json)!;
    // var goblin = new Goblin(data);
    public Player(PlayerData data) : this(data.Name, data.MaxHP, data.AttackPower) { }

}