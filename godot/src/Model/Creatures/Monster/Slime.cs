using DungeonDelver.Model.Creatures.CreatureData;

namespace DungeonDelver.Model.Creatures.Monster;

public class Slime : Creature
{
    
    // For JSON purposes ->
    // var data = JsonSerializer.Deserialize<CreatureData>(json)!;
    // var goblin = new Goblin(data);
    public Slime(SlimeData data) : base(data.Name, data.MaxHP, data.AttackPower) { }

    public Slime(string name, int maxHP, int attackPower) : base(name, maxHP, attackPower) { }
}