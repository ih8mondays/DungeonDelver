using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using DungeonDelver.Core.Combat;
using DungeonDelver.Model.Creatures;
using Godot;

namespace DungeonDelver.Scenes.Combat;

// The combat screen: puts a view on each slot, then plays the fight one StepResult at a time.
// CombatEncounter does the whole step instantly, so the screen is never ahead of the numbers; it just replays them.
public partial class CombatScene : Control {
    public event Action<CombatState> CombatFinished;

    // StepResult names its creatures, so this is how a name finds its view.
    // Names are unique inside a room (the map numbers them), Add throws if they ever aren't.
    private readonly Dictionary<string, CombatantView> _views = [];
    private Player _player;
    private List<Creature> _monsters;
    private Label _roundLabel;

    public void Setup(Player player, List<Creature> monsters) {
        _player = player;
        _monsters = monsters;
    }

    public override void _Ready() {
        _roundLabel = GetNode<Label>("TopBar/RoundLabel");
        _ = Run();   // fire and forget: Run reports its own errors, and ends by raising CombatFinished
    }

    private async Task Run() {
        CombatState state;
        try {
            AddViews();
            state = await PlayEncounter(new CombatEncounter(_player, _monsters));
            await ShowResult(state);
        }
        catch (Exception e) {
            // Without this the player would be stuck on a dead screen. Stalemate leaves the room uncleared.
            GD.PushError(e.ToString());
            state = CombatState.Stalemate;
        }

        if (IsInstanceValid(this))   // the scene may have been removed while we were waiting
            CombatFinished?.Invoke(state);
    }

    // Back to front, so monster 0 (in front) is drawn on top.
    private void AddViews() {
        for (var i = _monsters.Count - 1; i >= 0; i--)
            AddView(_monsters[i], $"MonsterSlot{i}");
        AddView(_player, "PlayerSlot");
    }

    // Step, play it, repeat. Returns how the fight ended.
    private async Task<CombatState> PlayEncounter(CombatEncounter encounter) {
        CombatState state;
        do {
            var step = encounter.Step();
            await PlayStep(step);
            state = step.StateAfter;
        } while (state == CombatState.InProgress);
        return state;
    }

    private async Task ShowResult(CombatState state) {
        _roundLabel.Text = state switch {
            CombatState.Victory => "Victory!",
            CombatState.Defeat => "Defeat...",
            _ => "Stalemate",
        };
        await ToSignal(GetTree().CreateTimer(1.2), SceneTreeTimer.SignalName.Timeout);   // let the result sink in
    }

    // One attack, as a story: attacker steps in, target takes the hit, and maybe falls over.
    private async Task PlayStep(StepResult step) {
        var attacker = _views[step.Actor];
        var target = _views[step.Target];

        _roundLabel.Text = $"Round {step.Round}";
        await attacker.Lunge(target.Position);
        await target.Hit(step.TargetHp, step.Dealt);
        if (step.TargetDied)
            await target.Die();
    }

    private void AddView(Creature creature, string slotName) {
        var view = GD.Load<PackedScene>("res://src/Scenes/Combat/combatant_view.tscn").Instantiate<CombatantView>();
        view.Position = GetNode<Marker2D>(slotName).Position;
        AddChild(view);                                      // in the tree first, so its _Ready has run
        view.Setup(creature, TextureFor(creature));
        _views.Add(creature.Name, view);
    }

    // TODO: temporary. Creatures have no sprite id yet; ask the teammate for one.
    private static Texture2D TextureFor(Creature creature) {
        var file = creature.Name.StartsWith("Goblin") ? "Assets/goblin1.png"
                 : creature.Name.StartsWith("Slime") ? "Assets/slime1.png"
                 : "icon.svg";
        return GD.Load<Texture2D>($"res://{file}");
    }
}
