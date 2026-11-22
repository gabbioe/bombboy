namespace BombBoy.Models.Tiles
{
    public class DestructableTile : BasicTile
    {
        public int Health;
        public DestructableTile(string col, string formating, string type, bool canWalk, int health = 1) : base(col, formating, type, canWalk)
        {
            Health = health;
        }
        public override bool DoDamage()
        {
            Health--;
            return Health <= 0;
        }
    }
}
