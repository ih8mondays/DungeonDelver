namespace DungeonDelver.Model.Dungeon.Layout;

/// The knobs for TieredLayout. All distances are in pixels.
/// "record" = a class that's mostly just data. "init" = can only be set when it's created.
public record LayoutSettings
{
    /// Horizontal gap between one tier and the next.
    public float TierSpacing { get; init; } = 180f;

    /// Vertical gap between two rooms in the same tier.
    public float RoomSpacing { get; init; } = 120f;

    /// Random up/down wobble, so the map doesn't look like a spreadsheet. 0 = perfectly straight rows.
    public float Jitter { get; init; } = 0f;

    /// Same seed = same wobble every time. Handy while debugging.
    public int Seed { get; init; } = 0;

    public static LayoutSettings Default { get; } = new();
}
