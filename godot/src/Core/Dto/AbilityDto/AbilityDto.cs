using System.Text.Json.Serialization;

namespace DungeonDelver.Core.Dto.AbilityDto;

// One ability as written in data/abilities.json.
// Id is the key creatures use in a map ("basic_attack"), Name is what the player sees.
public record AbilityDto(
    [property: JsonRequired] string Id,
    [property: JsonRequired] string Name,
    [property: JsonRequired] int BaseDamage,
    [property: JsonRequired] int AttackPowerScalingPercent,
    [property: JsonRequired] int ProcChancePercent,
    [property: JsonRequired] string IconId,
    [property: JsonRequired] string SpriteId);
