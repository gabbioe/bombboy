using Bombboy.Models.Tiles;
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

        public BasicTile CreateDestructableTile()
        {
            return new DestructableTile("5c6b73", "B", "DestructableTile", false);
        }

        public BasicTile CreateWaterTile()
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
