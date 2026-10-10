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
    public static class MapTileFactory
    {
        public static Tile CreateFloorTile()
        {
            return new Tile("#dff9fb");
        }

        public static IndestructableTile CreateIndestructableTile()
        {
            return new IndestructableTile("d35400");
        }

        public static DestructableTile CreateDestructableTile()
        {
            return new DestructableTile("5c6b73");
        }

        public static WaterTile CreateWaterTile()
        {
            return new WaterTile("d4f1f9");
        }


        public static string GetFactoryProductColours()
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
