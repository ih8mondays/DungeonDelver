using System.Collections.Generic;
using System.Linq;
using DungeonDelver.Model.Dto.DungeonDto;

namespace DungeonDelver.Core.Loading;

/// all the rules a map file has to follow.
internal static class DungeonValidator
{
    internal const int MinVersion = 1;       // oldest map format we still read
    internal const int CurrentVersion = 2;   // v2 = rooms can have "monsters"
    internal const int MaxExits = 3;    // max doors per room

    /// returns every broken rule. empty list = map is fine
    internal static List<string> Validate(DungeonDto dto)
    {
        var problems = new List<string>();

        if (dto.Version < MinVersion || dto.Version > CurrentVersion)
            problems.Add($"Unsupported version {dto.Version} (this game reads versions {MinVersion}-{CurrentVersion}).");

        if (string.IsNullOrWhiteSpace(dto.Name))
            problems.Add("Map has no name.");

        if (dto.Rooms == null || dto.Rooms.Count == 0)
        {
            problems.Add("Map has no rooms.");
            return problems;  // nothing more to check
        }

        // round 1: each room on its own
        for (var i = 0; i < dto.Rooms.Count; i++)
        {
            var room = dto.Rooms[i];
            if (room == null)
            {
                problems.Add($"Room #{i} is null.");
                continue;
            }

            // name used in messages: the id, or the position (#2) if the id is missing
            var name = string.IsNullOrWhiteSpace(room.Id) ? $"#{i}" : $"'{room.Id}'";

            if (string.IsNullOrWhiteSpace(room.Id))
                problems.Add($"Room #{i} has no id.");

            if (!RoomFactory.IsKnownType(room.Type))
                problems.Add($"Room {name} has unknown type '{room.Type}'. Allowed: {string.Join(", ", RoomFactory.KnownTypes)}.");

            // monsters: only from version 2, only in combat rooms, no blank ids
            // (whether the ids are real creatures is checked once creatures get loaded)
            if (room.Monsters != null)
            {
                if (dto.Version < 2)
                    problems.Add($"Room {name} has monsters, which need version 2 (file is version {dto.Version}).");
                if (room.Type != "combat")
                    problems.Add($"Room {name} is a '{room.Type}' room and can't have monsters.");
                if (room.Monsters.Any(string.IsNullOrWhiteSpace))
                    problems.Add($"Room {name} has an empty monster id.");
            }

            if (room.Exits == null)
            {
                problems.Add($"Room {name} has no exits list (use [] for the end room).");
                continue;  // can't check exits without a list
            }

            if (room.Exits.Count > MaxExits)
                problems.Add($"Room {name} has {room.Exits.Count} exits (max {MaxExits}).");

            if (room.Exits.Contains(room.Id))
                problems.Add($"Room {name} has an exit to itself.");

            // group equal exits together, groups with more than 1 = duplicates
            foreach (var dup in room.Exits.GroupBy(e => e).Where(g => g.Count() > 1))
                problems.Add($"Room {name} lists exit '{dup.Key}' more than once.");
        }

        if (problems.Count > 0)
            return problems;  // round 2 and 3 need clean rooms, stop here

        // round 2: ids are unique and every exit points to a real room
        foreach (var dup in dto.Rooms.GroupBy(r => r.Id).Where(g => g.Count() > 1))
            problems.Add($"Room id '{dup.Key}' is used {dup.Count()} times.");

        // hashset = fast Contains()
        var ids = dto.Rooms.Select(r => r.Id).ToHashSet();
        foreach (var room in dto.Rooms)
        foreach (var exit in room.Exits.Where(e => !ids.Contains(e)))
            problems.Add($"Room '{room.Id}' has an exit to '{exit}', which doesn't exist.");

        if (problems.Count > 0)
            return problems;  // round 3 needs valid exits

        // round 3: the whole map. one end, one start, no loops
        var ends = dto.Rooms.Where(r => r.Exits.Count == 0).Select(r => r.Id).ToList();
        if (ends.Count != 1)
            problems.Add($"Map needs exactly 1 end room (a room with no exits), found {ends.Count}: [{string.Join(", ", ends)}].");

        var starts = FindStartIds(dto.Rooms);
        if (starts.Count != 1)
            problems.Add($"Map needs exactly 1 start room (a room no exit leads to), found {starts.Count}: [{string.Join(", ", starts)}].");

        var inLoop = FindRoomsInLoops(dto.Rooms);
        if (inLoop.Count > 0)
            problems.Add($"Exits must only lead forward, but these rooms are in a loop: [{string.Join(", ", inLoop)}].");

        // one start + one end + no loops = every room can be reached and every path ends at the end room
        return problems;
    }

    // start room = a room no exit points to
    internal static List<string> FindStartIds(List<RoomDto> rooms)
    {
        // SelectMany = flatten all the exit lists into one
        var targets = rooms.SelectMany(r => r.Exits).ToHashSet();
        return rooms.Where(r => !targets.Contains(r.Id)).Select(r => r.Id).ToList();
    }

    // finds rooms stuck in a loop (kahn's algorithm)
    // idea: keep removing rooms nothing points to. whatever is left is in a loop (or stuck behind one)
    // no loops = everything gets removed = empty list
    private static List<string> FindRoomsInLoops(List<RoomDto> rooms)
    {
        // how many exits point to each room
        var incoming = rooms.ToDictionary(r => r.Id, _ => 0);
        foreach (var exit in rooms.SelectMany(r => r.Exits))
            incoming[exit]++;

        var byId = rooms.ToDictionary(r => r.Id);
        // rooms with 0 incoming = ready to be removed
        var ready = new Queue<string>(incoming.Where(kv => kv.Value == 0).Select(kv => kv.Key));
        while (ready.Count > 0)
        {
            var id = ready.Dequeue();
            incoming.Remove(id);
            // removed room no longer points at its exits. one hits 0 -> ready too
            foreach (var exit in byId[id].Exits)
                if (--incoming[exit] == 0)
                    ready.Enqueue(exit);
        }

        // still in incoming = never hit 0 = in a loop
        return rooms.Select(r => r.Id).Where(incoming.ContainsKey).ToList();
    }
}
