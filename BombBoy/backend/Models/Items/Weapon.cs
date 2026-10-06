using BombBoy.Models;
using BombBoy.Models.Tiles;

namespace BombBoy.Models.Items
{
    public abstract class Weapon : IItem
    {
        public int X { get; set; }
        public int Y { get; set; }
        public int UsesCount { get; set; }
        public int MaxUses { get; set; }
        private int ID { get; set; }

        public Weapon(int x, int y, int maxUses)
        {
            X = x;
            Y = y;
            MaxUses = maxUses;
            UsesCount = maxUses;
            ID = new Random().Next(1000, 9999);
        }

        public abstract string GetEmoji();

        // assigns weapon to specific player
        public void Pickup(Player player)
        {
            player.CurrentWeapon = this;
            // remove from map happens in Match logic
        }

        public virtual void Use(Player player)
        {
            if (UsesCount > 0)
            {
                UsesCount--;
                ExecuteEffect(player);
            }
        }

        protected abstract void ExecuteEffect(Player player);

        public void Destroy()
        {
            // logic to remove from map
        }
    }
    
    // one use long range weapon
    public class LaserGun : Weapon
    {        
        public LaserGun(int x, int y) : base(x, y, 1) { }

        public override string GetEmoji() => "🔫";

        protected override void ExecuteEffect(Player player)
        {
            // logic handled by Match to spawn gun blast in a line the player is facing
        }
    }

    // 3 uses mid range (?) weapon
    public class ThrowableWeapon : Weapon
    {
        public ThrowableWeapon(int x, int y) : base(x, y, 3) { }

        public override string GetEmoji() => "🧨";

        protected override void ExecuteEffect(Player player)
        {
            // logic handled by Match to spawn a bomb 2 blocks away (allows to throw through walls)
        }
    }

    // 3 uses close range weapon
    public class Knife : Weapon
    {
        public Knife(int x, int y) : base(x, y, 3) { }
        public override string GetEmoji() => "🔪";
        protected override void ExecuteEffect(Player player)
        {
            // breaking block/killing player logic will be handled in Match
        }
    }

    // concrete factory nr. 2
    public class Popsicle : Knife
    {
        public Popsicle(int x, int y) : base(x, y)
        {
            MaxUses = 2;
            UsesCount = 2;
        }
        public override string GetEmoji() => "🍦";
    }

    public class FrozenLaserGun : LaserGun
    {
        public FrozenLaserGun(int x, int y) : base(x, y) { }
        public override string GetEmoji() => "🍧";

        protected override void ExecuteEffect(Player player)
        {
            // bus tarp match.cs
        }
    }

    public class SnowballWeapon : ThrowableWeapon
    {
        public SnowballWeapon(int x, int y) : base(x, y) { }
        public override string GetEmoji() => "⚪";
    }
}