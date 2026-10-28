using System;
using System.Collections.Generic;
using DungeonDelver.Core.Combat;
using DungeonDelver.Core.Loading.Abilities;
using DungeonDelver.Model.Creatures;
using DungeonDelver.Scenes.Combat;
using DungeonDelver.Scenes.Dungeon;
using Godot;

namespace DungeonDelver.Scenes;

// Always-loaded root. Owns the Player so HP survives scene changes, and shows either the map or a fight.
public partial class Main : Node {
    public Player Player { get; set; }   // settable so "Restart" can replace it (use NewPlayer for a fresh one)
    public AbilityCatalog Abilities { get; private set; }   // data/abilities.json, the map looks creatures' abilities up in it

    private DungeonRun _dungeon;   // kept alive during a fight so the map's progress survives

    public override void _Ready() {
        Abilities = AbilityCatalog.FromJson(FileAccess.GetFileAsString("res://data/abilities.json"), "res://data/abilities.json");
        Player = NewPlayer();

        _dungeon = GD.Load<PackedScene>("res://src/Scenes/Dungeon/dungeon_run.tscn").Instantiate<DungeonRun>();
        _dungeon.Abilities = Abilities;               // both set before AddChild: the map loads in its _Ready
        _dungeon.FightRequested += StartCombat;
        AddChild(_dungeon);
    }

    // onFinished tells the map how the fight ended (it uses it to clear the room on Victory)
    private void StartCombat(List<Creature> monsters, Action<CombatState> onFinished) {
        var combat = GD.Load<PackedScene>("res://src/Scenes/Combat/combat_scene.tscn").Instantiate<CombatScene>();
        combat.Setup(Player, monsters);
        combat.CombatFinished += state => {   // later: game over on Defeat
            combat.QueueFree();
            SetDungeonActive(true);
            onFinished(state);
        };
        SetDungeonActive(false);
        AddChild(combat);
    }

    // The hero as a run starts. A fight can't start without at least one ability, so the basic attack goes on here.
    public Player NewPlayer() {
        var player = new Player("Ih8mondays", 100, 10);
        player.AddAbility(Abilities.Create("basic_attack"));
        return player;
    }

    private void SetDungeonActive(bool active) {
        _dungeon.Visible = active;
        _dungeon.ProcessMode = active ? ProcessModeEnum.Inherit : ProcessModeEnum.Disabled;   // stops its per-frame redraw too
    }
}
