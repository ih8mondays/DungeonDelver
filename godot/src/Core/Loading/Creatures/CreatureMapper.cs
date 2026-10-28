using DungeonDelver.Core.Dto.CreatureDto;
using DungeonDelver.Core.Loading.Abilities;

namespace DungeonDelver.Core.Loading.Creatures;

internal static class CreatureMapper {
    // only safe after the validator said OK, same contract as RoomFactory.Create
    internal static Model.Creatures.Creature ToCreature(CreatureDto dto, AbilityCatalog abilities = null) {
        var creature = new Model.Creatures.Creature(dto.Name, dto.MaxHp, dto.AttackPower);
        foreach (var id in dto.Abilities ?? [])
            creature.AddAbility(abilities!.Create(id));
        return creature;
    }
}
