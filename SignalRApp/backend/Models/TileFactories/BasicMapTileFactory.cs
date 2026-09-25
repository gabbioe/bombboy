using SignalRApp.Models.Tiles;
using System;
using System.Collections.Generic;
using System.Text;

namespace SignalRApp.Models.TileFactories
{
    internal class BasicMapTileFactory : IMapTileFactory
    {
        public FloorTile CreateFloorTile()
        {
            return new FloorTile("#27ae60");
        }

        public DestructableTile CreateDestructableTile()
        {
            return new DestructableTile("#d35400");
        }

        public IndestructableTile CreateIndestructableTile()
        {
            return new IndestructableTile("#5c6b73");
        }
    }
}
