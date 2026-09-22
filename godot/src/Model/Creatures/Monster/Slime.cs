using DungeonDelver.Model.Creatures.CreatureDto;

namespace DungeonDelver.Model.Creatures.Monster;

public class Slime(string name, int maxHp, int attackPower) : Creature(name, maxHp, attackPower)
{
    // For JSON purposes ->
    // var data = JsonSerializer.Deserialize<CreatureData>(json)!;
    // var goblin = new Goblin(data);
    public Slime(SlimeDto data) : this(data.Name, data.MaxHp, data.AttackPower) { }
}
