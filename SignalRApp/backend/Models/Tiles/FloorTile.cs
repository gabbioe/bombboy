using System;
using System.Collections.Generic;
using System.Text;

namespace SignalRApp.Models.Tiles
{
    public class FloorTile : Tile
    {
        public FloorTile(string col) :base()
        {
            _colour = col;
            _jsonFormating = "_";
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
