using System;
using DungeonDelver.Core.Dto.CreatureDto;
using DungeonDelver.Core.Loading.Abilities;
using DungeonDelver.Core.Loading.Dungeon;

namespace DungeonDelver.Core.Loading.Creatures;

public static class CreatureLoader {
    public static Model.Creatures.Creature Build(CreatureDto dto, string source = "(dto)", AbilityCatalog abilities = null) {
        ArgumentNullException.ThrowIfNull(dto);
        var problems = CreatureValidator.Validate(dto, abilities);
        return problems.Count > 0
            ? throw new InvalidDungeonException(source, problems)
            : CreatureMapper.ToCreature(dto, abilities);
    }
}
