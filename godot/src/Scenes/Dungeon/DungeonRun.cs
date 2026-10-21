using System;
using System.Collections.Generic;
using System.Linq;
using DungeonDelver.Model.Creatures;
using DungeonDelver.Model.Dungeon;
using DungeonDelver.Core.Loading;
using DungeonDelver.Core.Layout;
using DungeonDelver.Core.Loading.Dungeon;
using Godot;
using FileAccess = Godot.FileAccess;

namespace DungeonDelver.Scenes.Dungeon;

// The dungeon map screen: draws the whole map and lets you walk it by clicking a room.
// Positions come from TieredLayout (plain C#), this file only turns them into pixels and input.
// Still rough - hardcoded map path, circles instead of room art.

public partial class DungeonRun : Control {
    [Export(PropertyHint.File, "*.json")]
    public string MapPath { get; set; } = "res://maps/crypt_of_the_ember_king.json";

    [Export] public float TierSpacing { get; set; } = 180f;
    [Export] public float RoomSpacing { get; set; } = 120f;
    [Export] public float Jitter { get; set; } = 0f;
    [Export] public int Seed { get; set; } = 0;
    [Export] public float RoomRadius { get; set; } = 26f;

    /// How long the token takes to slide from one room to the next, in seconds.
    [Export] public float MoveTime { get; set; } = 0.35f;

    private static readonly Color Background = new("14121a");
    private static readonly Color EdgeDim = new("2e2b3a");
    private static readonly Color EdgeWalked = new("6f6a88");
    private static readonly Color EdgeOpen = new("9ad3e8");
    private static readonly Color CombatColor = new("b4463c");
    private static readonly Color ShopColor = new("d2a24c");
    private static readonly Color UnknownColor = new("6a6a6a");
    private static readonly Color Unvisited = new("2a2733");
    private static readonly Color CurrentRing = new("ffffff");
    private static readonly Color EndRing = new("7ad7f0");
    private static readonly Color Token = new("f4f1ff");
    private static readonly Color TextColor = new("e8e6f0");
    private static readonly Color TextDim = new("6f6a88");

    private DungeonMap _map;
    private MapLayout _layout;

    private Room _current;
    private readonly HashSet<string> _visited = new();

    // edges we've actually walked, as "from->to", so we can draw the trail
    private readonly HashSet<string> _walked = new();

    // where the token is, in LAYOUT space (no window offset baked in - the window can resize mid-move)
    private Vector2 _token;
    private bool _moving;

    private Room _hovered;
    private float _time;

    private Label _info;
    private VBoxContainer _buttons;

    public override void _Ready() {
        var box = new VBoxContainer {
            Position = new Vector2(16, 16),
            MouseFilter = MouseFilterEnum.Ignore,   // let clicks fall through to the map behind it
        };
        AddChild(box);

        _info = new Label();
        box.AddChild(_info);

        _buttons = new VBoxContainer();
        box.AddChild(_buttons);

        Resized += QueueRedraw;

        Load();
    }

    // the pulsing ring on the rooms you can walk to needs a repaint every frame
    public override void _Process(double delta) {
        _time += (float)delta;
        if (_map != null)
            QueueRedraw();
    }

    // ---- loading / walking ----

    private void Load() {
        try {
            var json = FileAccess.GetFileAsString(MapPath);
            if (FileAccess.GetOpenError() != Error.Ok)
                throw new InvalidDungeonException(MapPath, new[] { $"Could not open file: {FileAccess.GetOpenError()}" });

            _map = DungeonLoader.FromJson(json, MapPath);
            _layout = TieredLayout.Build(_map, new LayoutSettings {
                TierSpacing = TierSpacing,
                RoomSpacing = RoomSpacing,
                Jitter = Jitter,
                Seed = Seed,
            });
        }
        catch (InvalidDungeonException e) {
            GD.PushError(e.Message);
            _map = null;
            _layout = null;
            _info.Text = e.Message;
            ClearButtons();
            AddButton("Try again", Load);
            QueueRedraw();
            return;
        }

        _visited.Clear();
        _walked.Clear();
        _moving = false;
        _hovered = null;
        _token = _layout.Of(_map.Start);

        EnterRoom(_map.Start);
    }

    private void EnterRoom(Room room) {
        _current = room;
        _visited.Add(room.Id);
        room.Enter();
        Refresh();
    }

    /// Slides the token over, THEN enters the room. Input is locked while it slides.
    private void MoveTo(Room next) {
        if (_moving || _current == null) return;

        _walked.Add($"{_current.Id}->{next.Id}");
        _moving = true;
        Refresh();

        var tween = CreateTween();
        tween.TweenMethod(Callable.From<Vector2>(SetToken), _layout.Of(_current), _layout.Of(next), MoveTime)
             .SetTrans(Tween.TransitionType.Sine)
             .SetEase(Tween.EaseType.InOut);
        tween.Finished += () => {
            _moving = false;
            EnterRoom(next);
        };
    }

    private void SetToken(Vector2 position) {
        _token = position;
        QueueRedraw();
    }

    /// The rooms you're allowed to click right now.
    private IReadOnlyList<Room> Open() {
        if (_map == null || _current == null || _moving || !_current.CanLeave)
            return Array.Empty<Room>();

        return _map.ExitsOf(_current);
    }

    private void Refresh() {
        ClearButtons();
        if (_map == null) return;

        var status = _current switch {
            CombatRoom c => c.IsCleared ? "combat, cleared" : "combat, NOT cleared",
            ShopRoom => "shop",
            _ => _current.GetType().Name,
        };

        var hint = _moving ? "moving..."
            : !_current.CanLeave ? "clear the room before you can leave"
            : _current.IsEnd ? "end of the dungeon!"
            : "click a lit-up room to move there";

        _info.Text = $"{_map.Name}  -  tier {_layout.TierOf(_current)} of {_layout.TierCount - 1}\n"
                   + $"Room: {_current.Id} ({status})\n{hint}";

        if (!_moving && _current is CombatRoom combat && !combat.IsCleared)
            AddButton("Clear room", () => { combat.Clear(); Refresh(); });

        if (!_moving)   // TODO: temporary demo, remove when CombatRoom holds creatures
            AddButton("Fight (demo)", () =>
                (GetParent() as Main)?.StartCombat([new Creature("Goblin 1", 30, 5), new Creature("Slime", 20, 3)]));

        if (!_moving && _current.IsEnd)
            AddButton("Restart", Load);

        QueueRedraw();
    }

    // ---- mouse ----

    public override void _GuiInput(InputEvent @event) {
        if (_map == null) return;

        if (@event is InputEventMouseMotion motion) {
            var was = _hovered;
            _hovered = RoomAt(motion.Position);
            MouseDefaultCursorShape = _hovered != null && Open().Contains(_hovered)
                ? CursorShape.PointingHand
                : CursorShape.Arrow;

            if (was != _hovered) QueueRedraw();
            return;
        }

        if (@event is InputEventMouseButton { Pressed: true, ButtonIndex: MouseButton.Left } click) {
            var room = RoomAt(click.Position);
            if (room != null && Open().Contains(room))
                MoveTo(room);
        }
    }

    /// Which room is under this screen point? null if none.
    private Room RoomAt(Vector2 point) {
        var offset = Offset();
        return _map.Rooms.FirstOrDefault(r => (_layout.Of(r) + offset).DistanceTo(point) <= RoomRadius);
    }

    // ---- drawing ----

    public override void _Draw() {
        DrawRect(new Rect2(Vector2.Zero, Size), Background);
        if (_map == null || _layout == null) return;

        var offset = Offset();
        var open = Open();
        var pulse = 0.55f + 0.45f * Mathf.Sin(_time * 4f);   // 0.1 .. 1.0, for the breathing ring

        // 1. every edge, dim - then the ones we walked, then the ones we can take now
        foreach (var room in _map.Rooms) {
            var from = _layout.Of(room) + offset;
            foreach (var next in _map.ExitsOf(room)) {
                var to = _layout.Of(next) + offset;

                var walked = _walked.Contains($"{room.Id}->{next.Id}");
                var canTake = room == _current && open.Contains(next);

                var color = canTake ? EdgeOpen with { A = pulse } : walked ? EdgeWalked : EdgeDim;
                DrawLine(from, to, color, canTake || walked ? 3f : 2f, true);
                DrawArrow(from, to, color);
            }
        }

        var font = GetThemeDefaultFont();

        // 2. rooms
        foreach (var room in _map.Rooms) {
            var centre = _layout.Of(room) + offset;
            var seen = _visited.Contains(room.Id);
            var canTake = open.Contains(room);

            // unvisited rooms stay dark until you've been there or can go there next
            DrawCircle(centre, RoomRadius, seen || canTake ? FillFor(room) : Unvisited);

            if (canTake)
                DrawArc(centre, RoomRadius + 5f, 0, Mathf.Tau, 32, EdgeOpen with { A = pulse }, 3f, true);

            if (room == _hovered && canTake)
                DrawArc(centre, RoomRadius + 9f, 0, Mathf.Tau, 32, CurrentRing, 2f, true);

            if (room == _current && !_moving)
                DrawArc(centre, RoomRadius, 0, Mathf.Tau, 32, CurrentRing, 3f, true);
            else if (room.IsEnd)
                DrawArc(centre, RoomRadius, 0, Mathf.Tau, 32, EndRing, 2f, true);

            const float labelWidth = 160f;
            var labelPos = centre + new Vector2(-labelWidth / 2f, RoomRadius + 16f);
            DrawString(font, labelPos, room.Id, HorizontalAlignment.Center, labelWidth, 13,
                seen || canTake ? TextColor : TextDim);
        }

        // 3. the party token, on top of everything
        DrawCircle(_token + offset, 9f, Token);
    }

    /// Shifts the map so its middle lands in the middle of the window.
    private Vector2 Offset() => (Size - _layout.Size) / 2f - _layout.Min;

    private void DrawArrow(Vector2 from, Vector2 to, Color color) {
        var direction = (to - from).Normalized();
        var tip = to - direction * RoomRadius;
        var side = direction.Orthogonal() * 6f;

        DrawColoredPolygon(new[] { tip, tip - direction * 14f + side, tip - direction * 14f - side }, color);
    }

    private static Color FillFor(Room room) => room switch {
        CombatRoom => CombatColor,
        ShopRoom => ShopColor,
        _ => UnknownColor,
    };

    // ---- the button strip, same as DungeonWalk had ----

    private void AddButton(string text, Action onPressed) {
        var button = new Button { Text = text };
        button.Pressed += onPressed;
        _buttons.AddChild(button);
    }

    private void ClearButtons() {
        foreach (var child in _buttons.GetChildren()) {
            _buttons.RemoveChild(child);
            child.QueueFree();
        }
    }
}
