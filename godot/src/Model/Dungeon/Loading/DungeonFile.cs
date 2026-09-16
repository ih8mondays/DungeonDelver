using System.Collections.Generic;

namespace DungeonDelver.Model.Dungeon.Loading;

/// The shape of a map .json file. Only data, no behaviour.
/// DungeonLoader reads the JSON into this, then builds the real rooms from it.
public class DungeonFile
{
    /// File format version. Bump it when the format changes, so old maps can still be read.
    public required int Version { get; set; }

    public required string Name { get; set; }

    public required List<RoomEntry> Rooms { get; set; }

    /// One room as written in the file.
    public class RoomEntry
    {
        /// Unique name of the room, e.g. "cistern".
        public required string Id { get; set; }

        /// "combat" or "shop".
        public required string Type { get; set; }

        /// Ids of the rooms you can walk to next. Empty list = the end room.
        public required List<string> Exits { get; set; }
    }
}
