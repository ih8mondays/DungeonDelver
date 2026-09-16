using System;
using System.Collections.Generic;
using System.Linq;

namespace DungeonDelver.Model.Dungeon;

public class DungeonMap
{
    private readonly Dictionary<string, Room> _rooms;

    public string Name { get; }

    public Room Start { get; }

    // => means this property is calculated each time its read, rather than stored
    public IReadOnlyCollection<Room> Rooms => _rooms.Values;

    public DungeonMap(string name, IEnumerable<Room> rooms, string startId)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(name);
        ArgumentNullException.ThrowIfNull(rooms);

        Name = name;
        _rooms = rooms.ToDictionary(r => r.Id);
        Start = Get(startId);
    }
    
    public Room Get(string id) =>
        _rooms.TryGetValue(id, out var room)
            ? room
            : throw new KeyNotFoundException($"No room with id '{id}' in map '{Name}'.");

    public IReadOnlyList<Room> ExitsOf(Room room) => room.Exits.Select(Get).ToList();
}
