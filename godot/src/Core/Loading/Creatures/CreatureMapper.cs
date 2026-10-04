using DungeonDelver.Core.Dto.CreatureDto;

namespace DungeonDelver.Core.Loading.Creatures;

internal static class CreatureMapper {
    // only safe after the validator said OK, same contract as RoomFactory.Create
    internal static Model.Creatures.Creature ToCreature(CreatureDto dto) =>
        new(dto.Name, dto.MaxHp, dto.AttackPower);
}