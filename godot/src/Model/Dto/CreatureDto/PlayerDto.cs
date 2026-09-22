using System.Text.Json.Serialization;

namespace DungeonDelver.Model.Dto.CreatureDto;

// For JSON purposes ->
// var data = JsonSerializer.Deserialize<CreatureData>(json)!;
// var goblin = new Goblin(data);
public record PlayerDto(
    [property: JsonRequired] string Name, 
    [property: JsonRequired] int MaxHp, 
    [property: JsonRequired] int AttackPower);
