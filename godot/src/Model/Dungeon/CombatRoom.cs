using System.Collections.Generic;

namespace DungeonDelver.Model.Dungeon;

/// A room with a fight. You can't leave until it's cleared.
public class CombatRoom : Room
{
    // TODO: creatures to fight, once the Creature class exists.
    
    /// Not virtual, so subclasses can't override it (C#'s equivalent of Java's final).
    public bool IsCleared { get; private set; }

    public CombatRoom(string id, IEnumerable<string> exits) : base(id, exits)
    {
    }
    
    /// sealed override: a future subclass can't loosen the "cleared first" rule.
    public sealed override bool CanLeave => IsCleared;

    public override void Enter()
    {
        // TODO: start the fight against the creatures.
    }
    
    /// Finishes the room so the player can leave.
    /// For now called from a debug button; later called when the last creature dies.
    public void Clear() => IsCleared = true;
}