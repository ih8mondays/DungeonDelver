using DungeonDelver.Model.Creatures.CreatureData;

namespace DungeonDelver.Model.Creatures.Monster;

public class Slime(string name, int maxHP, int attackPower) : Creature(name, maxHP, attackPower)
{
    // For JSON purposes ->
    // var data = JsonSerializer.Deserialize<CreatureData>(json)!;
    // var goblin = new Goblin(data);
    public Slime(SlimeData data) : this(data.Name, data.MaxHP, data.AttackPower) { }
}
