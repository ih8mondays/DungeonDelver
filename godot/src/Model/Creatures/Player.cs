using DungeonDelver.Model.Creatures.CreatureData;

namespace DungeonDelver.Model.Creatures;

public class Player : Creature { 
    
    // For JSON purposes ->
    // var data = JsonSerializer.Deserialize<CreatureData>(json)!;
    // var goblin = new Goblin(data);
    public Player(PlayerData data) : base(data.Name, data.MaxHP, data.AttackPower) { }

    public Player(string name, int maxHP, int attackPower) : base(name, maxHP, attackPower) { }

}