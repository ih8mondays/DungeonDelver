using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace DungeonDelver.Model.Dungeon.Loading;

/// Turns map JSON into a DungeonMap:
///
///   JSON text --Parse--> DungeonFile --Validate, then build rooms--> DungeonMap
///
/// Plain C#, no Godot, so it can be unit tested. Reading the file from disk is the caller's job.
public static class DungeonLoader
{
    /// The map format version this code understands.
    public const int CurrentVersion = 1;

    /// Most exits a single room may have.
    public const int MaxExits = 3;

    /// Room type in the file -> how to build that room. Add a line here for a new room type.
    private static readonly Dictionary<string, Func<DungeonFile.RoomEntry, Room>> RoomFactories = new()
    {
        ["combat"] = r => new CombatRoom(r.Id, r.Exits),
        ["shop"] = r => new ShopRoom(r.Id, r.Exits),
    };

    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,            // "exits" in the file <-> Exits in C#
        ReadCommentHandling = JsonCommentHandling.Skip,               // allow // comments
        AllowTrailingCommas = true,
        UnmappedMemberHandling = JsonUnmappedMemberHandling.Disallow, // a typo like "exit" is an error
    };

    /// JSON text -> DungeonMap. `source` (e.g. the file name) is only used in error messages.
    public static DungeonMap FromJson(string json, string source = "(json)") =>
        Build(Parse(json, source), source);

    /// DungeonFile -> DungeonMap. Checks the map rules first and throws listing every problem.
    public static DungeonMap Build(DungeonFile file, string source = "(file)")
    {
        ArgumentNullException.ThrowIfNull(file);

        var problems = Validate(file);
        if (problems.Count > 0)
            throw new InvalidDungeonException(source, problems);

        var rooms = file.Rooms.Select(r => RoomFactories[r.Type](r));
        return new DungeonMap(file.Name, rooms, FindStartIds(file.Rooms).Single());
    }

    private static DungeonFile Parse(string json, string source)
    {
        try
        {
            return JsonSerializer.Deserialize<DungeonFile>(json, JsonOptions)
                   ?? throw new InvalidDungeonException(source, new[] { "File is empty (null)." });
        }
        catch (JsonException e)
        {
            throw new InvalidDungeonException(source, new[] { $"Bad JSON: {e.Message}" }, e);
        }
    }

    /// Returns every broken map rule. Empty list = the map is fine.
    private static List<string> Validate(DungeonFile file)
    {
        var problems = new List<string>();

        if (file.Version != CurrentVersion)
            problems.Add($"Unsupported version {file.Version} (this game reads version {CurrentVersion}).");

        if (string.IsNullOrWhiteSpace(file.Name))
            problems.Add("Map has no name.");

        if (file.Rooms == null || file.Rooms.Count == 0)
        {
            problems.Add("Map has no rooms.");
            return problems;
        }

        // 1. Each room on its own.
        for (var i = 0; i < file.Rooms.Count; i++)
        {
            var room = file.Rooms[i];
            if (room == null)
            {
                problems.Add($"Room #{i} is null.");
                continue;
            }

            var name = string.IsNullOrWhiteSpace(room.Id) ? $"#{i}" : $"'{room.Id}'";

            if (string.IsNullOrWhiteSpace(room.Id))
                problems.Add($"Room #{i} has no id.");

            if (room.Type == null || !RoomFactories.ContainsKey(room.Type))
                problems.Add($"Room {name} has unknown type '{room.Type}'. Allowed: {string.Join(", ", RoomFactories.Keys)}.");

            if (room.Exits == null)
            {
                problems.Add($"Room {name} has no exits list (use [] for the end room).");
                continue;
            }

            if (room.Exits.Count > MaxExits)
                problems.Add($"Room {name} has {room.Exits.Count} exits (max {MaxExits}).");

            if (room.Exits.Contains(room.Id))
                problems.Add($"Room {name} has an exit to itself.");

            foreach (var dup in room.Exits.GroupBy(e => e).Where(g => g.Count() > 1))
                problems.Add($"Room {name} lists exit '{dup.Key}' more than once.");
        }

        if (problems.Count > 0)
            return problems; // the checks below need well-formed rooms

        // 2. Ids are unique, and every exit points to a room that exists.
        foreach (var dup in file.Rooms.GroupBy(r => r.Id).Where(g => g.Count() > 1))
            problems.Add($"Room id '{dup.Key}' is used {dup.Count()} times.");

        var ids = file.Rooms.Select(r => r.Id).ToHashSet();
        foreach (var room in file.Rooms)
        foreach (var exit in room.Exits.Where(e => !ids.Contains(e)))
            problems.Add($"Room '{room.Id}' has an exit to '{exit}', which doesn't exist.");

        if (problems.Count > 0)
            return problems;

        // 3. The whole map: one end, one start, forward only.
        var ends = file.Rooms.Where(r => r.Exits.Count == 0).Select(r => r.Id).ToList();
        if (ends.Count != 1)
            problems.Add($"Map needs exactly 1 end room (a room with no exits), found {ends.Count}: [{string.Join(", ", ends)}].");

        var starts = FindStartIds(file.Rooms);
        if (starts.Count != 1)
            problems.Add($"Map needs exactly 1 start room (a room no exit leads to), found {starts.Count}: [{string.Join(", ", starts)}].");

        var inLoop = FindRoomsInLoops(file.Rooms);
        if (inLoop.Count > 0)
            problems.Add($"Exits must only lead forward, but these rooms are in a loop: [{string.Join(", ", inLoop)}].");

        // One start + one end + no loops means every room is reachable and every path ends at the end room.
        return problems;
    }

    /// Rooms that no exit points to.
    private static List<string> FindStartIds(List<DungeonFile.RoomEntry> rooms)
    {
        var targets = rooms.SelectMany(r => r.Exits).ToHashSet();
        return rooms.Where(r => !targets.Contains(r.Id)).Select(r => r.Id).ToList();
    }

    /// Kahn's algorithm: keep removing rooms nothing points to.
    /// Whatever can't be removed is in (or stuck behind) a loop.
    private static List<string> FindRoomsInLoops(List<DungeonFile.RoomEntry> rooms)
    {
        var incoming = rooms.ToDictionary(r => r.Id, _ => 0);
        foreach (var exit in rooms.SelectMany(r => r.Exits))
            incoming[exit]++;

        var byId = rooms.ToDictionary(r => r.Id);
        var ready = new Queue<string>(incoming.Where(kv => kv.Value == 0).Select(kv => kv.Key));
        while (ready.Count > 0)
        {
            var id = ready.Dequeue();
            incoming.Remove(id);
            foreach (var exit in byId[id].Exits)
                if (--incoming[exit] == 0)
                    ready.Enqueue(exit);
        }

        return rooms.Select(r => r.Id).Where(incoming.ContainsKey).ToList();
    }
}

/// Thrown when a map can't be loaded. Problems lists everything that's wrong, not just the first thing.
public class InvalidDungeonException : Exception
{
    public IReadOnlyList<string> Problems { get; }

    public InvalidDungeonException(string source, IEnumerable<string> problems)
        : this(source, problems.ToList(), null)
    {
    }

    public InvalidDungeonException(string source, IEnumerable<string> problems, Exception inner)
        : this(source, problems.ToList(), inner)
    {
    }

    private InvalidDungeonException(string source, List<string> problems, Exception inner)
        : base($"Map '{source}' is invalid:\n - " + string.Join("\n - ", problems), inner)
    {
        Problems = problems;
    }
}
