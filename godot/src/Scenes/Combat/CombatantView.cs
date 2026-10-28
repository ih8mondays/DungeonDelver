using System.Threading.Tasks;
using DungeonDelver.Model.Creatures;
using Godot;

namespace DungeonDelver.Scenes.Combat;

// One creature on screen: sprite, name, HP bar. It doesn't listen to anything.
// CombatScene tells it what to play, one awaitable animation at a time, using the numbers from a StepResult.
public partial class CombatantView : Node2D {
    private Sprite2D _sprite;
    private ProgressBar _hpBar;
    private Label _nameLabel;

    [Export] public float SpriteHeight { get; set; } = 160;   // on-screen height in pixels, whatever the image size
    [Export] public float LungeDistance { get; set; } = 40;   // how far the attacker steps toward its target

    public override void _Ready() {
        _sprite = GetNode<Sprite2D>("Sprite");
        _hpBar = GetNode<ProgressBar>("HpBar");
        _nameLabel = GetNode<Label>("NameLabel");
    }

    // Call after the view is in the scene tree, so _Ready has run.
    public void Setup(Creature creature, Texture2D texture) {
        _sprite.Texture = texture;
        _sprite.Scale = Vector2.One * (SpriteHeight / texture.GetHeight());
        _sprite.Offset = new Vector2(0, -texture.GetHeight() / 2f);   // feet on the view's position
        _nameLabel.Text = creature.Name;
        _hpBar.MaxValue = creature.MaxHp;
        _hpBar.Value = creature.CurrentHp;
    }

    // Steps toward the target and back.
    public async Task Lunge(Vector2 target) {
        var start = Position;
        var forward = start + (target - start).Normalized() * LungeDistance;
        var tween = CreateTween();
        tween.TweenProperty(this, "position", forward, 0.12).SetTrans(Tween.TransitionType.Quad).SetEase(Tween.EaseType.Out);
        tween.TweenProperty(this, "position", start, 0.18);
        await ToSignal(tween, Tween.SignalName.Finished);
    }

    // HP bar slides to the new value, sprite flashes red, damage number floats up. All at once.
    public async Task Hit(int newHp, int dealt) {
        var tween = CreateTween().SetParallel();
        tween.TweenProperty(_hpBar, "value", (double)newHp, 0.3);
        if (dealt > 0)
            tween.TweenProperty(_sprite, "modulate", Colors.White, 0.3).From(Colors.Red);
        FloatText($"-{dealt}");
        await ToSignal(tween, Tween.SignalName.Finished);
    }

    public async Task Die() {
        var tween = CreateTween();
        tween.TweenProperty(this, "modulate:a", 0.0, 0.5);
        await ToSignal(tween, Tween.SignalName.Finished);
    }

    // Fire and forget: the label cleans itself up, nobody waits for it.
    private void FloatText(string text) {
        var label = new Label { Text = text, Position = new Vector2(-10, -SpriteHeight) };
        AddChild(label);
        var tween = label.CreateTween().SetParallel();
        tween.TweenProperty(label, "position:y", label.Position.Y - 40, 0.6);
        tween.TweenProperty(label, "modulate:a", 0.0, 0.6);
        tween.Finished += label.QueueFree;
    }
}
