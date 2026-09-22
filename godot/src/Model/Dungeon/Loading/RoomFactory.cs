using System;
using System.Collections.Generic;
using DungeonDelver.Model.Dungeon.Loading.Dto;

namespace DungeonDelver.Model.Dungeon.Loading;

// "type" in the JSON -> the Room class it builds.
// new room type = add one line to Factories, nothing else changes.
internal static class RoomFactory
{
    // Func<in, out> = delegate. same as java Function<RoomDto, Room>
    private static readonly Dictionary<string, Func<RoomDto, Room>> Factories = new() {
        ["combat"] = r => new CombatRoom(r.Id, r.Exits),
        ["shop"] = r => new ShopRoom(r.Id, r.Exits)
    };

    // the type names a map file is allowed to use. the validator reads this
    internal static IReadOnlyCollection<string> KnownTypes => Factories.Keys;

    internal static bool IsKnownType(string type) => type != null && Factories.ContainsKey(type);

    // look up the build function for the type and call it with the room.
    // only safe after the validator said the type is known
    internal static Room Create(RoomDto entry) => Factories[entry.Type](entry);
}
