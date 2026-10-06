using BombBoy.Models;

namespace BombBoy.Models.Items
{
    public abstract class Powerup : IItem
    {
        public int X { get; set; }
        public int Y { get; set; }
        private int ID { get; set; }

        public Powerup(int x, int y)
        {
            X = x;
            Y = y;
            ID = new Random().Next(1000, 9999);
        }

        public abstract string GetEmoji();

        // uses the powerup on the player that picked it up
        public void Pickup(Player player)
        {            
            Use(player);
        }

        protected abstract void Use(Player player);

        public void Destroy()
        {
            // logic to remove item hit by blast radius or weapon from map (handled by Match)
        }
    }

    public class StatPowerup : Powerup
    {
        public string AffectedStat { get; set; }

        public StatPowerup(int x, int y, string stat) : base(x, y)
        {
            AffectedStat = stat;
        }

        public override string GetEmoji()
        {
            return AffectedStat switch
            {
                "BombRange" => "↔️",
                "BombCount" => "➕",
                _ => "❓"
            };
        }

        // updates specific player's stats according to the powerup type (both stats capped at 9)
        protected override void Use(Player player)
        {
            if (AffectedStat == "BombRange")
            {
                player.BombRange = Math.Min(player.BombRange + 1, 9);
            }
            else if (AffectedStat == "BombCount")
            {
                player.BombCount = Math.Min(player.BombCount + 1, 9);
            }
        }
    }

    // negates damage player gets if powerup duration hasn't exceeded the shield's max duration
    public class ShieldPowerup : Powerup
    {
        public int Duration { get; set; }
        public int MaxDuration { get; set; } = 10; // default shield duration is 10 seconds

        public ShieldPowerup(int x, int y, int duration = 10) : base(x, y)
        {
            Duration = duration;
        }

        public override string GetEmoji() => "🛡";

        protected override void Use(Player player)
        {
            player.HasShield = true;
            player.ShieldDuration = Duration;
        }

        public void Update(float deltaTime)
        {
            if (Duration > 0)
            {
                Duration -= (int)deltaTime;
            }
        }
    }

    public class ExtraLifePowerup : Powerup
    {
        public ExtraLifePowerup(int x, int y) : base(x, y) { }

        public override string GetEmoji() => "❤️";

        protected override void Use(Player player)
        {
            player.Lives = Math.Min(player.Lives + 1, 3); // capped at 3
            Console.WriteLine($"Player {player.Name} gained an extra life! Total: {player.Lives}");
        }
    }

    // concrete factory nr. 2
    public class FrozenShieldPowerup : ShieldPowerup
    {
        public FrozenShieldPowerup(int x, int y) : base(x, y) { }
        public override string GetEmoji() => "🧥";
    }

    public class FrozenStatPowerup : StatPowerup
    {
        public FrozenStatPowerup(int x, int y, string stat) : base(x, y, stat) { }
        public override string GetEmoji() => AffectedStat == "BombRange" ? "↕️" : "✨";
    }

}