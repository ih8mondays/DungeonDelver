using System.Collections.Generic;
using System.Linq;

namespace DungeonDelver.Model.Dungeon;

public class CombatRoom : Room
{
    // ids of the creatures to fight, e.g. ["goblin", "slime"].
    // ids, not Monster objects: fresh monsters get built by CreatureFactory when the fight starts.
    public IReadOnlyList<string> MonsterIds { get; }
    
    public bool IsCleared { get; private set; }
    public sealed override bool CanLeave => IsCleared;   // Only leave if cleared


    public CombatRoom(string id, IEnumerable<string> exits, IEnumerable<string> monsterIds = null) : base(id, exits) {
        MonsterIds = (monsterIds ?? []).ToArray();   // own copy, same as exits
    }
    
    public void Clear() => IsCleared = true;
    

    public override void Enter() {
        // TODO: start the fight against the creatures.
    }
    
}
