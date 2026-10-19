using BombBoy.Models.Tiles;
using System;
using System.Collections.Generic;
using System.Text;

namespace Bombboy.Models.Tiles
{
    public class WaterTile : BasicTile
    {
        public WaterTile(string col, string formating, string type, bool canwalk) :base(col, formating, type, canwalk)
        {
            //_colour = col;
            //_jsonFormating = "W";
            //Type = "WaterTile";
        }

        public override bool DoDamage()
        {
            return false;
        }
        public bool CanDrown()
        {
            return true;
        }
    }
}
