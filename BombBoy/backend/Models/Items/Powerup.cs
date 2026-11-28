using System;
using System.Collections.Generic;
using BombBoy.Models;
using BombBoy.Models.Composite;
using BombBoy.Models.Decorator;
using BombBoy.Models.Flyweight;
using BombBoy.Models.Strategies;
using BombBoy.Models.Visitor;

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
            ID = Random.Shared.Next(1000, 9999);
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

        public void Accept(IMatchVisitor visitor) => visitor.VisitItem(this);
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
            player.Decorate(c => new ImmortalDecorator(c, Duration));
        }

        public void Update(float deltaTime)
        {
            if (Duration > 0)
            {
                Duration -= (int)deltaTime;
            }
        }
    }

    public class WallPassPowerup : Powerup
    {
        public WallPassPowerup(int x, int y) : base(x, y) { }

        public override string GetEmoji() => "👻";

        protected override void Use(Player player)
        {
            player.Decorate(c => new WallPassDecorator(c));
        }
    }

    public class SpeedPowerup : Powerup
    {
        public SpeedPowerup(int x, int y) : base(x, y) { }

        public override string GetEmoji() => "⚡";

        protected override void Use(Player player)
        {
            player.Decorate(c => new SpeedDecorator(c, 1));
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

    /// <summary>
    /// Dynamically upgrades a player's ActiveBombStrategy into a Composite tree (or deeper nested Composite)
    /// by fusing an additional Flyweight leaf explosion strategy into their current bomb pattern.
    /// </summary>
    public class BombFusionPowerup : Powerup
    {
        private static readonly (string key, Type checkType)[] CandidateShapes =
        {
            ("ring", typeof(HollowRingLeaf)),
            ("knight", typeof(KnightScatterLeaf)),
            ("checker", typeof(CheckerboardLeaf)),
            ("diagonal", typeof(DiagonalExplosionStrategy)),
            ("piercing", typeof(PiercingLineExplosionStrategy))
        };

        public BombFusionPowerup(int x, int y) : base(x, y) { }

        public override string GetEmoji() => "🧬";

        protected override void Use(Player player)
        {
            var current = player.ActiveBombStrategy ?? ExplosionStrategyFlyweightFactory.GetStrategy("classic");
            var missingKeys = new List<string>();

            foreach (var (key, checkType) in CandidateShapes)
            {
                bool alreadyPresent = current is CompositeExplosionStrategy comp
                    ? comp.ContainsStrategyType(checkType)
                    : current is ExplosionLeaf leaf
                        ? leaf.InnerStrategy.GetType() == checkType
                        : current.GetType() == checkType;

                if (!alreadyPresent)
                {
                    missingKeys.Add(key);
                }
            }

            if (missingKeys.Count > 0)
            {
                string chosenKey = missingKeys[Random.Shared.Next(missingKeys.Count)];
                player.ApplyPowerUp(CompositeBombPresets.Fuse(current, chosenKey));
            }
            else
            {
                player.BombRange = Math.Min(player.BombRange + 1, 9);
            }
        }
    }

    public class FrozenBombFusionPowerup : BombFusionPowerup
    {
        public FrozenBombFusionPowerup(int x, int y) : base(x, y) { }
        public override string GetEmoji() => "💠";
    }

}
