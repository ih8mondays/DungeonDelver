using System;
using System.Collections.Generic;
using System.Linq;
using DungeonDelver.Model.Creatures;

namespace DungeonDelver.Model.Dungeon;

public class CombatRoom : Room
{
    // the monsters waiting in this room. read-only from outside, like Room.Exits
    public IReadOnlyList<Creature> Creatures { get; }

    public bool IsCleared { get; private set; }
    public sealed override bool CanLeave => IsCleared;   // Only leave if cleared

    public CombatRoom(string id, IEnumerable<string> exits, IEnumerable<Creature> creatures) : base(id, exits) {
        ArgumentNullException.ThrowIfNull(creatures);
        Creatures = creatures.ToArray();   // own copy, same reason as Room.Exits
    }

    public void Clear() => IsCleared = true;

    public override void Enter() {
        // Nothing to do here: the model can't open a screen. DungeonRun sees we were entered and starts the fight.
    }

}
