using BombBoy.Models.Tiles;
using System;
using System.Collections.Generic;
using System.Text;

namespace Bombboy.Models.Tiles
{
    public class WaterTile : Tile
    {
        public WaterTile(string col) :base(col)
        {
            _colour = col;
            _jsonFormating = "W";
            Type = "WaterTile";
        }

        public override bool CanWalk()
        {
            return true;
        }

        public override bool DoDamage()
        {
            return false;
        }
        public static bool CanDrown()
        {
            return true;
        }
    }
}
