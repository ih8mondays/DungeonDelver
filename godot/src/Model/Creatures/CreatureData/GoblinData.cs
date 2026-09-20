namespace DungeonDelver.Model.Creatures.CreatureData;

// For JSON purposes ->
// var data = JsonSerializer.Deserialize<CreatureData>(json)!;
// var goblin = new Goblin(data);
public record GoblinData(string Name, int MaxHP, int AttackPower);
