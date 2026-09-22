using System;
using System.Collections.Generic;
using System.Linq;
using Godot;

namespace DungeonDelver.Model.Dungeon.Layout;

/// Where every room sits on screen, and which tier it belongs to.
/// Just the answer - the work happens in TieredLayout.
///
/// Vector2 is a plain struct from GodotSharp (managed math, no engine behind it), so this class
/// still runs in a normal xUnit test. What needs the running engine is Node / Resource / the scene
/// tree, and none of that is in here.
public class MapLayout
{
    private readonly Dictionary<string, Vector2> _positions;
    private readonly Dictionary<string, int> _tiers;

    /// Number of tiers, so 0..TierCount-1.
    public int TierCount { get; }

    /// Corners of the box all rooms fit inside. The view uses these to centre the map.
    public Vector2 Min { get; }
    public Vector2 Max { get; }

    /// How wide and tall the whole map is.
    public Vector2 Size => Max - Min;

    public MapLayout(Dictionary<string, Vector2> positions, Dictionary<string, int> tiers)
    {
        ArgumentNullException.ThrowIfNull(positions);
        ArgumentNullException.ThrowIfNull(tiers);

        _positions = positions;
        _tiers = tiers;
        TierCount = tiers.Count == 0 ? 0 : tiers.Values.Max() + 1;

        Min = new Vector2(positions.Values.Min(p => p.X), positions.Values.Min(p => p.Y));
        Max = new Vector2(positions.Values.Max(p => p.X), positions.Values.Max(p => p.Y));
    }

    public Vector2 Of(Room room) => Of(room.Id);

    public Vector2 Of(string roomId) =>
        _positions.TryGetValue(roomId, out var point)
            ? point
            : throw new KeyNotFoundException($"No position for room '{roomId}'.");

    /// 0 = start room. A room's tier is 1 + the tier of its deepest predecessor.
    public int TierOf(Room room) => TierOf(room.Id);

    public int TierOf(string roomId) =>
        _tiers.TryGetValue(roomId, out var tier)
            ? tier
            : throw new KeyNotFoundException($"No tier for room '{roomId}'.");
}
