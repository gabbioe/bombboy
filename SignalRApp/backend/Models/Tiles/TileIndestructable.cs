using System;
using System.Collections.Generic;
using System.Text;

namespace SignalRApp.Models.Tiles
{
    public class TileIndestructable : Tile
    {
        public TileIndestructable(string col) :base()
        {
            colour = col;
            JsonFormating = "X";
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
