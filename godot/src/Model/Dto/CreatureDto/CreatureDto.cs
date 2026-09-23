using System.Text.Json.Serialization;

namespace DungeonDelver.Model.Dto.CreatureDto;

/// CreatureFactory builds the real Monster from it.
public record CreatureDto(
    [property: JsonRequired] string Id,          // template id, e.g. "goblin"
    [property: JsonRequired] string Name,        // shown in game, e.g. "Goblin"
    [property: JsonRequired] int MaxHp,
    [property: JsonRequired] int AttackPower,
    string Sprite = null);                       // sprite key, e.g. "goblin1". optional -> Creature.DefaultSpriteId
