namespace DungeonDelver.Model.Creatures.Monster;

public class Goblin : Creature
{
    
    // For JSON purposes ->
    // var data = JsonSerializer.Deserialize<CreatureData>(json)!;
    // var goblin = new Goblin(data);
    public Goblin(CreatureData data) : base(data) { }

    public Goblin(string name, int maxHP, int attackPower) : base(name, maxHP, attackPower) { }
}