using SignalRApp.Models.Tiles;
using System;
using System.Collections.Generic;
using System.Text;

namespace SignalRApp.Models.TileFactories
{
    internal class FactoryMapTileBasic : IFactoryMapTile
    {
        public TileFloor CreateFloorTile()
        {
            return new TileFloor("#dff9fb");
        }

        public TileDestructable CreateDestructableTile()
        {
            return new TileDestructable("#d35400");
        }

        public TileIndestructable CreateIndestructableTile()
        {
            return new TileIndestructable("#5c6b73");
        }
    }
}
