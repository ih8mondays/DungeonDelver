using System.Collections.Generic;
using DungeonDelver.Model.Creatures;
using DungeonDelver.Scenes.Combat;
using Godot;

namespace DungeonDelver.Scenes;

// Always-loaded root. Owns the Player so HP survives scene changes, and swaps the current screen.
public partial class Main : Node {
    public Player Player { get; } = new("Ih8mondays", 100, 10);

    private Node _current;

    public override void _Ready() => ShowDungeon();

    public void ShowDungeon() =>
        Show(GD.Load<PackedScene>("res://src/Scenes/Dungeon/dungeon_run.tscn").Instantiate());

    public void StartCombat(List<Creature> monsters) {
        var combat = GD.Load<PackedScene>("res://src/Scenes/Combat/combat_scene.tscn").Instantiate<CombatScene>();
        combat.Setup(Player, monsters);
        combat.CombatFinished += _ => ShowDungeon();   // later: mark room cleared, or game over on Defeat
        Show(combat);
    }

    private void Show(Node next) {
        _current?.QueueFree();
        _current = next;
        AddChild(next);
    }
}
