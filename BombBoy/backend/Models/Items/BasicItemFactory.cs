using BombBoy.Models.Items;

namespace BombBoy.Models.ItemFactories
{
    public class BasicItemFactory : IAbstractItemFactory
    {
        private Random _rand = new Random();

        public Powerup CreatePowerup(int x, int y)
        {
            // randomly choose between stat, shield, and extra life
            int choice = _rand.Next(0, 3);
            return choice switch
            {
                0 => new StatPowerup(x, y, _rand.Next(0, 2) == 0 ? "BombRange" : "BombCount"),
                1 => new ShieldPowerup(x, y),
                _ => new ExtraLifePowerup(x, y)
            };
        }

        public Weapon CreateWeapon(int x, int y)
        {
            // randomly choose between laser gun, throwable, and knife
            int choice = _rand.Next(0, 3);
            return choice switch
            {
                0 => new LaserGun(x, y),
                1 => new ThrowableWeapon(x, y),
                _ => new Knife(x, y)
            };
        }
    }
}