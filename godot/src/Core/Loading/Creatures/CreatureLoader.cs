using System;
using DungeonDelver.Core.Dto.CreatureDto;
using DungeonDelver.Core.Loading.Dungeon;

namespace DungeonDelver.Core.Loading.Creatures;

public static class CreatureLoader {
    public static Model.Creatures.Creature Build(CreatureDto dto, string source = "(dto)") {
        ArgumentNullException.ThrowIfNull(dto);
        var problems = CreatureValidator.Validate(dto);
        return problems.Count > 0 
            ? throw new InvalidDungeonException(source, problems) 
            : CreatureMapper.ToCreature(dto);
    }
}