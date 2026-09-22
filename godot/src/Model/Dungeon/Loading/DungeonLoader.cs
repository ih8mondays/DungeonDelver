using System;
using System.Linq;

namespace DungeonDelver.Model.Dungeon.Loading;

/// json text -> DungeonFile -> check rules -> DungeonMap
public static class DungeonLoader {
    
    // re-exported so callers don't have to know where the rules live
    public const int CurrentVersion = DungeonValidator.CurrentVersion;
    public const int MaxExits = DungeonValidator.MaxExits;
    
    
    public static DungeonMap FromJson(string json, string source = "(json)") =>
        Build(DungeonJsonParser.Parse(json, source), source);

    
    
    public static DungeonMap Build(DungeonFile file, string source = "(file)")
    {
        ArgumentNullException.ThrowIfNull(file);

        var problems = DungeonValidator.Validate(file);
        if (problems.Count > 0)
            throw new InvalidDungeonException(source, problems);

        var rooms = file.Rooms.Select(RoomFactory.Create);
        
        return new DungeonMap(file.Name, rooms, DungeonValidator.FindStartIds(file.Rooms).Single());
    }
}
