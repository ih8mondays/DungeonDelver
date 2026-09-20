namespace DungeonDelver.Model.Creatures;

public class Player : Creature { 
    
    // For JSON purposes ->
    // var data = JsonSerializer.Deserialize<CreatureData>(json)!;
    // var goblin = new Goblin(data);
    public Player(CreatureData data) : base(data) { }

    public Player(string name, int maxHP, int attackPower) : base(name, maxHP, attackPower) { }

}