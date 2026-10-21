using System;
using System.Collections.Generic;
using DungeonDelver.Core.Combat;
using DungeonDelver.Model.Creatures;
using Godot;

namespace DungeonDelver.Scenes.Combat;

// Placeholder so Main compiles - filled in properly in Step 4.
public partial class CombatScene : Control {
    public event Action<CombatState> CombatFinished;

    public void Setup(Player player, List<Creature> monsters) { }
}
