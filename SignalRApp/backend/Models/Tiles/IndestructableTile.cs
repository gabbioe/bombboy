using System;
using System.Collections.Generic;
using System.Text;

namespace SignalRApp.Models.Tiles
{
    public class IndestructableTile : Tile
    {
        public IndestructableTile(string col) :base()
        {
            _colour = col;
            _jsonFormating = "X";
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
