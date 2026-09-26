using System;
using System.Collections.Generic;
using System.Text;

namespace SignalRApp.Models.Tiles
{
    public class TileDestructable : Tile
    {
        public TileDestructable(string col) :base()
        {
            colour = col;
            JsonFormating = "B";
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
