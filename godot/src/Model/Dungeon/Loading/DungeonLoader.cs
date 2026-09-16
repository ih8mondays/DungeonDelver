using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace DungeonDelver.Model.Dungeon.Loading;

// json text -> DungeonFile -> check rules -> DungeonMap
// no godot in here, so it can be unit tested. reading the file from disk is godot's job (DungeonWalk)
// static class = only static methods, you can't make objects of it
public static class DungeonLoader {
    // map format version we can read. bump it when the json format changes
    public const int CurrentVersion = 1;

    // max doors per room
    public const int MaxExits = 3;

    // "type" in the json -> function that builds that room
    // Func<in, out> = delegate. same as java Function<RoomEntry, Room>
    // new room type = add one line here
    private static readonly Dictionary<string, Func<DungeonFile.RoomEntry, Room>> RoomFactories = new() {
        ["combat"] = r => new CombatRoom(r.Id, r.Exits),
        ["shop"] = r => new ShopRoom(r.Id, r.Exits),
    };

    // settings for the .NET json reader
    private static readonly JsonSerializerOptions JsonOptions = new() {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,              // "exits" in json = Exits in c#
        ReadCommentHandling = JsonCommentHandling.Skip,                 // allow // comments in map files
        AllowTrailingCommas = true,                                     // allow a comma after the last item
        UnmappedMemberHandling = JsonUnmappedMemberHandling.Disallow,  // unknown field (typo like "exit") = error
    };

    // main entry point. text in, map out
    // source = file name, only used in error messages. "(json)" is a default value, so it can be left out
    public static DungeonMap FromJson(string json, string source = "(json)") =>
        Build(Parse(json, source), source);

    // checks all the rules first. if anything is wrong -> throws ONE exception with every problem
    public static DungeonMap Build(DungeonFile file, string source = "(file)")
    {
        ArgumentNullException.ThrowIfNull(file);

        var problems = Validate(file);
        if (problems.Count > 0)
            throw new InvalidDungeonException(source, problems);

        // look up the build function for the type and call it with the room
        var rooms = file.Rooms.Select(r => RoomFactories[r.Type](r));
        // Single() = there must be exactly one start. Validate already made sure of that
        return new DungeonMap(file.Name, rooms, FindStartIds(file.Rooms).Single());
    }

    // json text -> DungeonFile (plain data, no real rooms yet)
    private static DungeonFile Parse(string json, string source)
    {
        try
        {
            // ?? = if the left side is null, do the right side
            return JsonSerializer.Deserialize<DungeonFile>(json, JsonOptions)
                   ?? throw new InvalidDungeonException(source, new[] { "File is empty (null)." });
        }
        // broken json -> rethrow as our own exception, so callers only catch one type
        catch (JsonException e)
        {
            throw new InvalidDungeonException(source, new[] { $"Bad JSON: {e.Message}" }, e);
        }
    }

    // returns every broken rule. empty list = map is fine
    // collects all problems instead of stopping at the first one
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
            return problems;  // nothing more to check
        }

        // round 1: each room on its own
        for (var i = 0; i < file.Rooms.Count; i++)
        {
            var room = file.Rooms[i];
            if (room == null)
            {
                problems.Add($"Room #{i} is null.");
                continue;
            }

            // name used in messages: the id, or the position (#2) if the id is missing
            var name = string.IsNullOrWhiteSpace(room.Id) ? $"#{i}" : $"'{room.Id}'";

            if (string.IsNullOrWhiteSpace(room.Id))
                problems.Add($"Room #{i} has no id.");

            if (room.Type == null || !RoomFactories.ContainsKey(room.Type))
                problems.Add($"Room {name} has unknown type '{room.Type}'. Allowed: {string.Join(", ", RoomFactories.Keys)}.");

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
        foreach (var dup in file.Rooms.GroupBy(r => r.Id).Where(g => g.Count() > 1))
            problems.Add($"Room id '{dup.Key}' is used {dup.Count()} times.");

        // hashset = fast Contains()
        var ids = file.Rooms.Select(r => r.Id).ToHashSet();
        foreach (var room in file.Rooms)
        foreach (var exit in room.Exits.Where(e => !ids.Contains(e)))
            problems.Add($"Room '{room.Id}' has an exit to '{exit}', which doesn't exist.");

        if (problems.Count > 0)
            return problems;  // round 3 needs valid exits

        // round 3: the whole map. one end, one start, no loops
        var ends = file.Rooms.Where(r => r.Exits.Count == 0).Select(r => r.Id).ToList();
        if (ends.Count != 1)
            problems.Add($"Map needs exactly 1 end room (a room with no exits), found {ends.Count}: [{string.Join(", ", ends)}].");

        var starts = FindStartIds(file.Rooms);
        if (starts.Count != 1)
            problems.Add($"Map needs exactly 1 start room (a room no exit leads to), found {starts.Count}: [{string.Join(", ", starts)}].");

        var inLoop = FindRoomsInLoops(file.Rooms);
        if (inLoop.Count > 0)
            problems.Add($"Exits must only lead forward, but these rooms are in a loop: [{string.Join(", ", inLoop)}].");

        // one start + one end + no loops = every room can be reached and every path ends at the end room
        return problems;
    }

    // start room = a room no exit points to
    private static List<string> FindStartIds(List<DungeonFile.RoomEntry> rooms)
    {
        // SelectMany = flatten all the exit lists into one
        var targets = rooms.SelectMany(r => r.Exits).ToHashSet();
        return rooms.Where(r => !targets.Contains(r.Id)).Select(r => r.Id).ToList();
    }

    // finds rooms stuck in a loop (kahn's algorithm)
    // idea: keep removing rooms nothing points to. whatever is left is in a loop (or stuck behind one)
    // no loops = everything gets removed = empty list
    private static List<string> FindRoomsInLoops(List<DungeonFile.RoomEntry> rooms)
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

// our own exception. Problems = everything that's wrong, not just the first thing
public class InvalidDungeonException : Exception
{
    public IReadOnlyList<string> Problems { get; }

    // the public constructors just pass on to the private one. ": this(...)" = java this(...)
    public InvalidDungeonException(string source, IEnumerable<string> problems)
        : this(source, problems.ToList(), null)
    {
    }

    public InvalidDungeonException(string source, IEnumerable<string> problems, Exception inner)
        : this(source, problems.ToList(), inner)
    {
    }

    // builds the message: "Map 'x' is invalid:" + one line per problem
    private InvalidDungeonException(string source, List<string> problems, Exception inner)
        : base($"Map '{source}' is invalid:\n - " + string.Join("\n - ", problems), inner)
    {
        Problems = problems;
    }
}
