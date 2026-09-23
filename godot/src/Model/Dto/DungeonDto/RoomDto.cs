using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace DungeonDelver.Model.Dto.DungeonDto;

/// One room as written in the file.
public record RoomDto(
    [property: JsonRequired] string Id, // Unique name of the room, e.g. "cistern".
    [property: JsonRequired] string Type,
    [property: JsonRequired] List<string> Exits); // Ids of the rooms you can walk to next. Empty list = the end room.