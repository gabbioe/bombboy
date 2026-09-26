using SignalRApp.Models.Tiles;
using System;
using System.Collections.Generic;
using System.Reflection.Metadata.Ecma335;
using System.Runtime.InteropServices;
using System.Text;
using System.Text.Json;
using System.Text.Json.Nodes;

namespace SignalRApp.Models.TileFactories
{
    public interface IFactoryMapTile
    {
        public abstract TileFloor CreateFloorTile();

        public abstract TileIndestructable CreateIndestructableTile();

        public abstract TileDestructable CreateDestructableTile();

        public virtual string GetFactoryProductColours()
        {
            var result = new JsonObject
            {
                ["floortile"] = CreateFloorTile().Colour(),
                ["indestructableTile"] = CreateIndestructableTile().Colour(),
                ["destructableTile"] = CreateDestructableTile().Colour(),
            };
            
            return JsonSerializer.Serialize(result);
        }
    }
}
