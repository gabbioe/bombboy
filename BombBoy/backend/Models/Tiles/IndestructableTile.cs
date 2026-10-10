using System;
using System.Collections.Generic;
using System.Text;

namespace BombBoy.Models.Tiles
{
    public class IndestructableTile : Tile
    {
        public IndestructableTile(string col) :base(col)
        {
            _colour = col;
            _jsonFormating = "X";
            Type = "IndestructableTile";
        }
        public override bool CanWalk()
        {
            return false;
        }

        public override bool DoDamage()
        {
            return false;
        }
    }
}
