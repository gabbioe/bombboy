using BombBoy.Models.Items;

namespace BombBoy.Models.ItemFactories
{
    // frozen theme items
    // tik emojis keičia tbh bet jei ateity keisim movement speeds tai praverstų debuffs nuo items pakurt
    public class FrozenItemFactory : IAbstractItemFactory
    {
        private Random _rand = new Random();

        public Powerup CreatePowerup(int x, int y)
        {
            // only spawns Frozen themed items (+ no extra life)
            int choice = _rand.Next(0, 2);
            return choice switch
            {
                0 => new FrozenStatPowerup(x, y, _rand.Next(0, 2) == 0 ? "BombRange" : "BombCount"),
                _ => new FrozenShieldPowerup(x, y)
            };
        }

        public Weapon CreateWeapon(int x, int y)
        {
            // only spawns Frozen themed items
            int choice = _rand.Next(0, 3);
            return choice switch
            {
                0 => new FrozenLaserGun(x, y),
                1 => new SnowballWeapon(x, y),
                _ => new Popsicle(x, y)
            };
        }
    }
}