using System;
using System.Collections.Generic;
using System.Linq;
using DungeonDelver.Model.Dungeon;
using Godot;

namespace DungeonDelver.Core.Layout;

/// Turns a DungeonMap into screen positions: tiers become columns, rooms inside a tier become rows.
/// No scene tree and no engine in here - only Vector2, which is plain managed math - so this runs
/// in a normal xUnit test. The view only asks "where does room X go?".
///
/// The loader already guarantees the map is a DAG (one start, one end, exits only lead forward),
/// so we don't check for loops again here.
public static class TieredLayout
{
    public static MapLayout Build(DungeonMap map, LayoutSettings settings = null)
    {
        ArgumentNullException.ThrowIfNull(map);
        settings ??= LayoutSettings.Default;   // ??= means "if it's null, set it to this"

        var tiers = Tiers(map);
        var order = OrderWithinTiers(map, tiers);
        var positions = Positions(tiers, order, settings);

        return new MapLayout(positions, tiers);
    }

    // ---- step 1: which column is each room in? ----

    /// tier(room) = 1 + max(tier of the rooms pointing at it). Start room = 0.
    /// Longest path, NOT shortest: with shortest paths a room could sit next to one of its own
    /// predecessors and the edge would run straight up/down instead of forward.
    /// Walking the rooms in topological order (kahn again) means every predecessor is done first.
    private static Dictionary<string, int> Tiers(DungeonMap map)
    {
        // how many exits point at each room
        var incoming = map.Rooms.ToDictionary(r => r.Id, _ => 0);
        foreach (var exit in map.Rooms.SelectMany(r => r.Exits))
            incoming[exit]++;

        var tier = map.Rooms.ToDictionary(r => r.Id, _ => 0);

        // rooms nothing points at are ready: that's the start room
        var ready = new Queue<Room>(map.Rooms.Where(r => incoming[r.Id] == 0));
        while (ready.Count > 0)
        {
            var room = ready.Dequeue();
            foreach (var next in map.ExitsOf(room))
            {
                // push the next room one column further right than this one, if it isn't already
                tier[next.Id] = Math.Max(tier[next.Id], tier[room.Id] + 1);

                // last predecessor done -> this room's tier is final, it can go in the queue
                if (--incoming[next.Id] == 0)
                    ready.Enqueue(next);
            }
        }

        return tier;
    }

    // ---- step 2: in what order do the rooms of one tier stack up? ----

    /// Returns each room's slot number inside its own tier (0 = topmost).
    /// A room is placed near the average slot of the rooms that lead into it ("barycentre"),
    /// which is the cheap standard trick for keeping the connecting lines from crossing.
    private static Dictionary<string, int> OrderWithinTiers(DungeonMap map, Dictionary<string, int> tiers)
    {
        // room -> the rooms that have an exit into it
        var predecessors = map.Rooms.ToDictionary(r => r.Id, _ => new List<string>());
        foreach (var room in map.Rooms)
        foreach (var exit in room.Exits)
            predecessors[exit].Add(room.Id);

        var byTier = map.Rooms
            .GroupBy(r => tiers[r.Id])
            .ToDictionary(g => g.Key, g => g.Select(r => r.Id).ToList());

        var order = new Dictionary<string, int>();

        // left to right, so the tier we're sorting always has its predecessors placed already
        foreach (var tier in byTier.Keys.OrderBy(t => t))
        {
            var rooms = byTier[tier];

            if (tier > 0)
                rooms = rooms.OrderBy(id => Barycentre(id)).ThenBy(id => id).ToList();

            for (var i = 0; i < rooms.Count; i++)
                order[rooms[i]] = i;
        }

        return order;

        // average slot of this room's predecessors. no predecessors (shouldn't happen past tier 0) -> 0
        double Barycentre(string id)
        {
            var slots = predecessors[id].Where(order.ContainsKey).Select(p => (double)order[p]).ToList();
            return slots.Count == 0 ? 0 : slots.Average();
        }
    }

    // ---- step 3: slot numbers -> actual pixels ----

    private static Dictionary<string, Vector2> Positions(
        Dictionary<string, int> tiers,
        Dictionary<string, int> order,
        LayoutSettings settings)
    {
        // how many rooms are in each tier, so we can centre them vertically
        var sizePerTier = tiers.Values.GroupBy(t => t).ToDictionary(g => g.Key, g => g.Count());

        var random = new Random(settings.Seed);
        var positions = new Dictionary<string, Vector2>();

        // fixed walk order (tier, then slot) so the same seed always gives the same wobble
        foreach (var id in tiers.Keys.OrderBy(id => tiers[id]).ThenBy(id => order[id]))
        {
            var tier = tiers[id];
            var slot = order[id];
            var count = sizePerTier[tier];

            var x = tier * settings.TierSpacing;

            // (slot - middle) * spacing -> tier of 1 sits at 0, tier of 3 sits at -1, 0, +1
            var y = (slot - (count - 1) / 2f) * settings.RoomSpacing;

            if (settings.Jitter > 0f)
                y += (float)(random.NextDouble() * 2 - 1) * settings.Jitter;   // -jitter .. +jitter

            positions[id] = new Vector2(x, y);
        }

        return positions;
    }
}
