using System;
using System.Collections.Generic;
using System.Linq;

namespace DungeonDelver.Model.Dungeon;

public abstract class Room
{
    public string Id { get; }
    
    public IReadOnlyList<string> Exits { get; }

    /// The single room with no exits is the end of the dungeon.
    public bool IsEnd => Exits.Count == 0;
    
    /// Whether the player may move on. Open by default; subclasses can lock it.
    public virtual bool CanLeave => true;

    protected Room(string id, IEnumerable<string> exits)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(id);
        ArgumentNullException.ThrowIfNull(exits);

        Id = id;
        Exits = exits.ToArray(); // copy, so the caller can't change our exits afterwards
    }
    
    /// What happens when the team walks in. Will take a Team once that class exists.
    public abstract void Enter();

    public override string ToString() => $"{GetType().Name}({Id} -> [{string.Join(", ", Exits)}])";
}