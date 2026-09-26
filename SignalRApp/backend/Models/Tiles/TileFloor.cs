using System;
using System.Collections.Generic;
using System.Text;

namespace SignalRApp.Models.Tiles
{
    public class TileFloor : Tile
    {
        public TileFloor(string col) :base()
        {
            colour = col;
            JsonFormating = "_";
        }
        public override bool CanWalk()
        {
            return true;
        }

        public override bool DoDamage()
        {
            return false;
        }
    }
}
