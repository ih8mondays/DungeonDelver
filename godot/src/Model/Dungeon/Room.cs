using System;
using System.Collections.Generic;
using System.Linq;

namespace DungeonDelver.Model.Dungeon;

public abstract class Room
{
    public string Id { get; }
    
    public IReadOnlyList<string> Exits { get; }

    public bool IsEnd => Exits.Count == 0;
    
    // combat room overwrites this to "once its cleared"
    public virtual bool CanLeave => true;

    // protected = only subclasses can call this constructor
    protected Room(string id, IEnumerable<string> exits)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(id);
        ArgumentNullException.ThrowIfNull(exits);

        Id = id;
        Exits = exits.ToArray();    // make our own copy of the exit names, so changes outside can't affect this room
    }
    
    public abstract void Enter();

    public override string ToString() => $"{GetType().Name}({Id} -> [{string.Join(", ", Exits)}])";
}
