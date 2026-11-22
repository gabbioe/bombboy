using BombBoy.Models.Tiles;

namespace Bombboy.Models.Tiles
{
    public class WaterTile : BasicTile
    {
        public WaterTile(string col, string formating, string type, bool canwalk) : base(col, formating, type, canwalk)
        {
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
