using System;
using System.Linq;
using DungeonDelver.Core.Dto.DungeonDto;
using DungeonDelver.Core.Loading.Abilities;
using DungeonDelver.Model.Dungeon;

namespace DungeonDelver.Core.Loading.Dungeon;

/// json text -> DungeonDto -> check rules -> DungeonMap
public static class DungeonLoader {
    
    // re-exported so callers don't have to know where the rules live
    public const int CurrentVersion = DungeonValidator.CurrentVersion;
    public const int MaxExits = DungeonValidator.MaxExits;
    
    
    // abilities = where creatures' ability ids get looked up. Optional: a map whose creatures list no abilities doesn't need it
    public static DungeonMap FromJson(string json, string source = "(json)", AbilityCatalog abilities = null) =>
        Build(DungeonJsonParser.Parse(json, source), source, abilities);



    public static DungeonMap Build(DungeonDto dto, string source = "(dto)", AbilityCatalog abilities = null)
    {
        ArgumentNullException.ThrowIfNull(dto);

        var problems = DungeonValidator.Validate(dto, abilities);
        if (problems.Count > 0)
            throw new InvalidDungeonException(source, problems);

        var rooms = dto.Rooms.Select(r => RoomFactory.Create(r, abilities));
        
        return new DungeonMap(dto.Name, rooms, DungeonValidator.FindStartIds(dto.Rooms).Single());
    }
}
