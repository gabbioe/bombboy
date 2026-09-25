using SignalRApp.Models.Tiles;
using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Text;

namespace SignalRApp.Models.TileFactories
{
    public interface IMapTileFactory
    {
        public abstract FloorTile CreateFloorTile();

        public abstract IndestructableTile CreateIndestructableTile();

        public abstract DestructableTile CreateDestructableTile();
    }
}
