using System;
using System.Collections.Generic;
using System.Text;

namespace BombBoy.Models.Tiles
{
    public class DestructableTile : Tile
    {
        public DestructableTile(string col) :base()
        {
            _colour = col;
            _jsonFormating = "B";
        }
        public override bool CanWalk()
        {
            return false;
        }

        public override bool DoDamage()
        {
            return true;
        }
    }
}
