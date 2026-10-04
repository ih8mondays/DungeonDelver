using System;
using System.Linq;
using DungeonDelver.Core.Dto.DungeonDto;
using DungeonDelver.Model.Dungeon;

namespace DungeonDelver.Core.Loading.Dungeon;

/// json text -> DungeonDto -> check rules -> DungeonMap
public static class DungeonLoader {
    
    // re-exported so callers don't have to know where the rules live
    public const int CurrentVersion = DungeonValidator.CurrentVersion;
    public const int MaxExits = DungeonValidator.MaxExits;
    
    
    public static DungeonMap FromJson(string json, string source = "(json)") =>
        Build(DungeonJsonParser.Parse(json, source), source);

    
    
    public static DungeonMap Build(DungeonDto dto, string source = "(dto)")
    {
        ArgumentNullException.ThrowIfNull(dto);

        var problems = DungeonValidator.Validate(dto);
        if (problems.Count > 0)
            throw new InvalidDungeonException(source, problems);

        var rooms = dto.Rooms.Select(RoomFactory.Create);
        
        return new DungeonMap(dto.Name, rooms, DungeonValidator.FindStartIds(dto.Rooms).Single());
    }
}
