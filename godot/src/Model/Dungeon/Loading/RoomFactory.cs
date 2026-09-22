using System;
using System.Collections.Generic;

namespace DungeonDelver.Model.Dungeon.Loading;

// "type" in the json -> the Room class it builds.
// new room type = add one line to Factories, nothing else changes.
internal static class RoomFactory
{
    // Func<in, out> = delegate. same as java Function<RoomEntry, Room>
    private static readonly Dictionary<string, Func<DungeonFile.RoomEntry, Room>> Factories = new() {
        ["combat"] = r => new CombatRoom(r.Id, r.Exits),
        ["shop"] = r => new ShopRoom(r.Id, r.Exits),
    };

    // the type names a map file is allowed to use. the validator reads this
    internal static IReadOnlyCollection<string> KnownTypes => Factories.Keys;

    internal static bool IsKnownType(string type) => type != null && Factories.ContainsKey(type);

    // look up the build function for the type and call it with the room.
    // only safe after the validator said the type is known
    internal static Room Create(DungeonFile.RoomEntry entry) => Factories[entry.Type](entry);
}
