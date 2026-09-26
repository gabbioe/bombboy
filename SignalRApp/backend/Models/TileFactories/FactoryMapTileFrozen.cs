using SignalRApp.Models.Tiles;
using System;
using System.Collections.Generic;
using System.Text;

namespace SignalRApp.Models.TileFactories
{
    internal class FactoryMapTileFrozen : IFactoryMapTile
    {
        public TileFloor CreateFloorTile()
        {
            return new TileFloor("#dff9fb"); // Pale Ice / Snow Floor
        }

        public TileDestructable CreateDestructableTile()
        {
            return new TileDestructable("#22a6b3"); // Bright Turquoise Ice Block
        }

        public TileIndestructable CreateIndestructableTile()
        {
            return new TileIndestructable("#130f40"); // Deep Frozen Rock Wall
        }
    }
}
