using System.Collections.Generic;

namespace DungeonDelver.Model.Dungeon;

/// A shop. Always safe to leave.
public class ShopRoom : Room
{
    // TODO: the shop, once it exists.

    public ShopRoom(string id, IEnumerable<string> exits) : base(id, exits)
    {
    }

    public override void Enter()
    {
        // TODO: open the shop.
    }
}