namespace DungeonDelver.Model.Creatures;

// For JSON purposes ->
// var data = JsonSerializer.Deserialize<CreatureData>(json)!;
// var goblin = new Goblin(data);
public record CreatureData(string Name, int MaxHP, int AttackPower);
    