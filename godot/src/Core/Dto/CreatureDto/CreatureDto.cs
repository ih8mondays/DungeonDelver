using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace DungeonDelver.Core.Dto.CreatureDto;

// For JSON purposes ->
// var data = JsonSerializer.Deserialize<CreatureData>(json)!;
// var goblin = new Goblin(data);
public record CreatureDto(
    [property: JsonRequired] string Name,
    [property: JsonRequired] int MaxHp,
    [property: JsonRequired] int AttackPower,
    List<string> Abilities = null); // Ids from data/abilities.json, e.g. ["basic_attack"]. Needs an AbilityCatalog to load.
