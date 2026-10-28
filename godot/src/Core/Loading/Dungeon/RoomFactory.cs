using System;
using System.Collections.Generic;
using System.Linq;
using DungeonDelver.Core.Dto.DungeonDto;
using DungeonDelver.Core.Loading.Abilities;
using DungeonDelver.Core.Loading.Creatures;
using DungeonDelver.Model.Dungeon;

namespace DungeonDelver.Core.Loading.Dungeon;

/// new room type = add one line to Factories, nothing else changes.
internal static class RoomFactory
{
    // Func<in, out> = delegate
    private static readonly Dictionary<string, Func<RoomDto, AbilityCatalog, Room>> Factories = new() {
        ["combat"] = (r, abilities) => new CombatRoom(r.Id, r.Exits, r.Creatures.Select(c => CreatureMapper.ToCreature(c, abilities))),
        ["shop"] = (r, _) => new ShopRoom(r.Id, r.Exits)
    };

    // the type names a map file is allowed to use. the validator reads this
    internal static IReadOnlyCollection<string> KnownTypes => Factories.Keys;

    internal static bool IsKnownType(string type) => type != null && Factories.ContainsKey(type);

    // look up the build function for the type and call it with the room.
    // only safe after the validator said the type is known
    internal static Room Create(RoomDto entry, AbilityCatalog abilities) => Factories[entry.Type](entry, abilities);
}
