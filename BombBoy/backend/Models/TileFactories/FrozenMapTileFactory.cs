using BombBoy.Models.Tiles;
using System;
using System.Collections.Generic;
using System.Text;

namespace BombBoy.Models.TileFactories
{
    internal class FrozenMapTileFactory : IMapTileFactory
    {
        public FloorTile CreateFloorTile()
        {
            return new FloorTile("#dff9fb"); // Pale Ice / Snow Floor
        }

        public DestructableTile CreateDestructableTile()
        {
            return new DestructableTile("#22a6b3"); // Bright Turquoise Ice Block
        }

        public IndestructableTile CreateIndestructableTile()
        {
            return new IndestructableTile("#130f40"); // Deep Frozen Rock Wall
        }
    }
}
