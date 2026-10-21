using System;
using System.Collections.Generic;
using DungeonDelver.Core.Combat;
using DungeonDelver.Model.Creatures;
using Godot;

namespace DungeonDelver.Scenes.Combat;

// The combat screen: puts a view on each slot and owns the subscriptions for the length of the fight.
public partial class CombatScene : Control {
    public event Action<CombatState> CombatFinished;

    private readonly AnimationQueue _queue = new();   // one queue for every view, so animations stay in order
    private readonly List<CombatantView> _views = [];
    private Player _player;
    private List<Creature> _monsters;

    public void Setup(Player player, List<Creature> monsters) {
        _player = player;
        _monsters = monsters;
    }

    public override void _Ready() {
        // Back to front, so monster 0 (in front) is drawn on top.
        for (var i = _monsters.Count - 1; i >= 0; i--)
            AddView(_monsters[i], $"MonsterSlot{i}");
        AddView(_player, "PlayerSlot");

        // TODO: step loop. While not finished: encounter.Step(); await _queue.Drain();
        // TODO: RoundLabel updates on the encounter's RoundStarted event.
    }

    // The step loop will call this when the encounter's Finished event arrives.
    public void Finish(CombatState state) {
        UnbindAll();
        CombatFinished?.Invoke(state);
    }

    // TODO: temporary debug keys, remove when the real step loop exists.
    // 1/2/3 hit that monster for 10, P hits the player for 10, Enter ends the fight.
    public override void _UnhandledKeyInput(InputEvent @event) {
        if (@event is not InputEventKey { Pressed: true, Echo: false } key) return;

        var index = (int)(key.Keycode - Key.Key1);
        if (index is >= 0 and <= 2 && index < _monsters.Count) _monsters[index].Damage(10);
        else if (key.Keycode == Key.P) _player.Damage(10);
        else if (key.Keycode == Key.Enter) Finish(CombatState.Victory);

        _ = _queue.Drain();   // debug only: the real loop awaits this between steps
    }

    // Safety net: if the scene is removed early, views must still stop listening.
    public override void _ExitTree() => UnbindAll();

    private void AddView(Creature creature, string slotName) {
        var view = GD.Load<PackedScene>("res://src/Scenes/Combat/combatant_view.tscn").Instantiate<CombatantView>();
        view.Position = GetNode<Marker2D>(slotName).Position;
        AddChild(view);                                      // in the tree first, so its _Ready has run
        view.Bind(creature, TextureFor(creature), _queue);
        _views.Add(view);
    }

    private void UnbindAll() {
        foreach (var view in _views) view.Unbind();
    }

    // TODO: temporary. Creatures have no sprite id yet; ask the teammate for one.
    private static Texture2D TextureFor(Creature creature) {
        var file = creature.Name.StartsWith("Goblin") ? "Assets/goblin1.png"
                 : creature.Name.StartsWith("Slime") ? "Assets/slime1.png"
                 : "icon.svg";
        return GD.Load<Texture2D>($"res://{file}");
    }
}
