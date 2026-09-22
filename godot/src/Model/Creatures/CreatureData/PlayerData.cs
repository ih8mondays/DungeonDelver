namespace DungeonDelver.Model.Creatures.CreatureData;

// For JSON purposes ->
// var data = JsonSerializer.Deserialize<CreatureData>(json)!;
// var goblin = new Goblin(data);
public record PlayerData(string Name, int MaxHP, int AttackPower);
