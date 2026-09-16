using System;
using DungeonDelver.Model.Dungeon;
using DungeonDelver.Model.Dungeon.Loading;
using Godot;
using FileAccess = Godot.FileAccess;

namespace DungeonDelver.Debug;

// ---temporary file for debugging dungeon map nativation. will be deleted later.---

public partial class DungeonWalk : Control {
    [Export(PropertyHint.File, "*.json")]
    public string MapPath { get; set; } = "res://maps/crypt_of_the_ember_king.json";

    private DungeonMap _map;
    private Room _current;
    private Label _info;
    private VBoxContainer _buttons;

    public override void _Ready() {
        var box = new VBoxContainer { Position = new Vector2(20, 20) };
        AddChild(box);

        _info = new Label();
        box.AddChild(_info);

        _buttons = new VBoxContainer();
        box.AddChild(_buttons);

        LoadMap();
    }

    private void LoadMap() {
        try {
            var json = FileAccess.GetFileAsString(MapPath);
            if (FileAccess.GetOpenError() != Error.Ok)
                throw new InvalidDungeonException(MapPath, new[] { $"Could not open file: {FileAccess.GetOpenError()}" });

            _map = DungeonLoader.FromJson(json, MapPath);
        }
        catch (InvalidDungeonException e) {
            GD.PushError(e.Message);
            _info.Text = e.Message;
            ClearButtons();
            AddButton("Try again", LoadMap);
            return;
        }

        EnterRoom(_map.Start);
    }

    private void EnterRoom(Room room) {
        _current = room;
        room.Enter();
        Refresh();
    }

    private void Refresh() {
        ClearButtons();

        var status = _current switch {
            CombatRoom c => c.IsCleared ? "combat, cleared" : "combat, not cleared",
            ShopRoom => "shop",
            _ => _current.GetType().Name,
        };
        _info.Text = $"{_map.Name}\nRoom: {_current.Id} ({status})";

        if (!_current.CanLeave) {
            if (_current is CombatRoom combat)
                AddButton("Clear room", () => { combat.Clear(); Refresh(); });
            return;
        }

        if (_current.IsEnd) {
            _info.Text += "\n\nEnd of the dungeon!";
            AddButton("Restart", LoadMap);
            return;
        }

        foreach (var next in _map.ExitsOf(_current))
            AddButton($"Go to {next.Id}", () => EnterRoom(next));
    }

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
