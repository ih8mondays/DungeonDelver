using System.Collections.Generic;
using DungeonDelver.Model.Creatures;
using DungeonDelver.Scenes.Combat;
using Godot;

namespace DungeonDelver.Scenes;

// Always-loaded root. Owns the Player so HP survives scene changes, and shows either the map or a fight.
public partial class Main : Node {
    public Player Player { get; set; } = new("Ih8mondays", 100, 10);   // settable so "Restart" can replace it

    private Control _dungeon;   // kept alive during a fight so the map's progress survives

    public override void _Ready() {
        _dungeon = GD.Load<PackedScene>("res://src/Scenes/Dungeon/dungeon_run.tscn").Instantiate<Control>();
        AddChild(_dungeon);
    }

    public void StartCombat(List<Creature> monsters) {
        var combat = GD.Load<PackedScene>("res://src/Scenes/Combat/combat_scene.tscn").Instantiate<CombatScene>();
        combat.Setup(Player, monsters);
        combat.CombatFinished += _ => {   // later: mark room cleared, or game over on Defeat
            combat.QueueFree();
            SetDungeonActive(true);
        };
        SetDungeonActive(false);
        AddChild(combat);
    }

    private void SetDungeonActive(bool active) {
        _dungeon.Visible = active;
        _dungeon.ProcessMode = active ? ProcessModeEnum.Inherit : ProcessModeEnum.Disabled;   // stops its per-frame redraw too
    }
}
