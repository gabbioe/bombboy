using BombBoy.Models.Tiles;
using System;
using System.Collections.Generic;
using System.Text;

namespace Bombboy.Models.Tiles
{
    internal class WaterTile : Tile
    {
        public WaterTile(string col) :base(col)
        {
            _colour = col;
            _jsonFormating = "W";
        }

        public override bool CanWalk()
        {
            return base.CanWalk();
        }

        public override bool DoDamage()
        {
            return base.DoDamage();
        }
    }
}
