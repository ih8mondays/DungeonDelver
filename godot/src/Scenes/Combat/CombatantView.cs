using System;
using DungeonDelver.Model.Creatures;
using Godot;

namespace DungeonDelver.Scenes.Combat;

// One creature on screen: sprite, name, HP bar. Listens to the creature's events and queues animations.
// Animations use the event's own data (never the creature's live HP), because they play later than the event fired.
public partial class CombatantView : Node2D {
    private Sprite2D _sprite;
    private ProgressBar _hpBar;
    private Label _nameLabel;
    private Creature _creature;
    private AnimationQueue _queue;

    [Export] public float SpriteHeight { get; set; } = 160;   // on-screen height in pixels, whatever the image size

    public override void _Ready() {
        _sprite = GetNode<Sprite2D>("Sprite");
        _hpBar = GetNode<ProgressBar>("HpBar");
        _nameLabel = GetNode<Label>("NameLabel");
    }

    // Call after the view is in the scene tree. Every += here needs a matching -= in Unbind.
    public void Bind(Creature creature, Texture2D texture, AnimationQueue queue) {
        _creature = creature;
        _queue = queue;
        _sprite.Texture = texture;
        _sprite.Scale = Vector2.One * (SpriteHeight / texture.GetHeight());
        _sprite.Offset = new Vector2(0, -texture.GetHeight() / 2f);   // feet on the view's position
        _nameLabel.Text = creature.Name;
        _hpBar.MaxValue = creature.MaxHp;
        _hpBar.Value = creature.CurrentHp;
        creature.HpChanged += OnHpChanged;
        creature.Died += OnDied;
    }

    public void Unbind() {
        if (_creature == null) return;
        _creature.HpChanged -= OnHpChanged;
        _creature.Died -= OnDied;
        _creature = null;
    }

    private void OnHpChanged(object sender, HpChangedEventArgs e) => _queue.Enqueue(() => {
        _hpBar.MaxValue = e.MaxHp;
        var tween = CreateTween().SetParallel();
        tween.TweenProperty(_hpBar, "value", (double)e.NewHp, 0.3);
        if (e.NewHp < e.OldHp)   // red flash only when damaged, not healed
            tween.TweenProperty(_sprite, "modulate", Colors.White, 0.3).From(Colors.Red);
        return tween;
    });

    private void OnDied(object sender, EventArgs e) => _queue.Enqueue(() => {
        var tween = CreateTween();
        tween.TweenProperty(this, "modulate:a", 0.0, 0.5);
        return tween;
    });
}
