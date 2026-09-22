using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace DungeonDelver.Model.Dto.DungeonDto;

// DungeonLoader reads the JSON into this, then builds the real rooms from it.
public record DungeonDto(
    [property: JsonRequired] int Version,
    [property: JsonRequired] string Name,
    [property: JsonRequired] List<RoomDto> Rooms);
