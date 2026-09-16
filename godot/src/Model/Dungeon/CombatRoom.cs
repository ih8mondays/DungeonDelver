using System.Collections.Generic;

namespace DungeonDelver.Model.Dungeon;

public class CombatRoom : Room
{
    // TODO: creatures to fight, once the Creature class exists.
    
    public bool IsCleared { get; private set; }

    public CombatRoom(string id, IEnumerable<string> exits) : base(id, exits) 
    { }
    
    // Later something like this:
    // public CombatRoom(string id, IEnumerable<string> exits, IEnumerable<Creature> creatures)
    //    : base(id, exits) {
    // _creatures = creatures.ToList(); }
    
    // Only leave if cleared
    public sealed override bool CanLeave => IsCleared;

    public override void Enter() {
        // TODO: start the fight against the creatures.
    }
    
    public void Clear() => IsCleared = true;
}
