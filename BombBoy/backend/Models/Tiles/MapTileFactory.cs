using Bombboy.Models.Tiles;
using BombBoy.Models.Tiles;
using System;
using System.Collections.Generic;
using System.Reflection.Metadata.Ecma335;
using System.Runtime.InteropServices;
using System.Text;
using System.Text.Json;
using System.Text.Json.Nodes;

namespace BombBoy.Models.Tiles
{
    public class MapTileFactory
    {
        public BasicTile CreateFloorTile()
        {
            return new BasicTile("#dff9fb", "_", "Tile", true);
        }

        public BasicTile CreateIndestructableTile()
        {
            return new BasicTile("d35400", "X", "IndestructableTile", false);
        }

        public DestructableTile CreateDestructableTile()
        {
            return new DestructableTile("5c6b73", "B", "DestructableTile", false);
        }

        public WaterTile CreateWaterTile()
        {
            return new WaterTile("d4f1f9", "W", "WaterTile", true);
        }


        public string GetFactoryProductColours()
        {
            var result = new JsonObject
            {
                ["floortile"] = CreateFloorTile().Colour(),
                ["indestructableTile"] = CreateIndestructableTile().Colour(),
                ["destructableTile"] = CreateDestructableTile().Colour(),
                ["WaterTile"] = CreateWaterTile().Colour(),
            };
            
            return JsonSerializer.Serialize(result);
        }
    }
}
