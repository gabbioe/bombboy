using BombBoy.Models;
using BombBoy.Models.Bridge;
using BombBoy.Models.Tiles;

namespace BombBoy.Models.Items
{
    /// <summary>
    /// Abstract Template Class defining the invariant weapon execution algorithm.
    /// </summary>
    public abstract class Weapon : IItem
    {
        public int X { get; set; }
        public int Y { get; set; }
        public int UsesCount { get; set; }
        public int MaxUses { get; set; }
        private int ID { get; set; }

        // Bridge Implementor Reference
        public IWeaponEffect Effect { get; set; }

        protected Weapon(int x, int y, int maxUses, IWeaponEffect? effect = null)
        {
            X = x;
            Y = y;
            MaxUses = maxUses;
            UsesCount = maxUses;
            ID = Random.Shared.Next(1000, 9999);
            Effect = effect ?? new FireExplosiveEffect();
        }

        public abstract string GetEmoji();

        public void Pickup(Player player)
        {
            player.CurrentWeapon = this;
        }

        /// <summary>
        /// Template Method (non-virtual/final): defines the fixed skeleton for using any weapon.
        /// Subclasses customize targeting and payload delivery without altering the overall lifecycle.
        /// </summary>
        public void Use(Player player, Match match, Map map)
        {
            if (!CanUse(player))
                return;

            match.NotifyWeaponUsed(player, GetType().Name);
            ConsumeUse();

            var (dx, dy) = GetDirectionOffset(player.Direction);
            List<(int x, int y)> targetTiles = SelectTargetTiles(player, map, dx, dy);
            ApplyWeaponPayload(player, match, map, targetTiles);

            OnAfterUse(player);
        }

        // Hook: subclasses may override precondition checks (e.g. cooldowns)
        protected virtual bool CanUse(Player player) => player.IsAlive && UsesCount > 0;

        // Invariant step: decrement remaining charges
        private void ConsumeUse() => UsesCount--;

        // Invariant step: map facing direction string to grid step vector
        protected static (int dx, int dy) GetDirectionOffset(string direction) => direction switch
        {
            "Up" => (0, -1),
            "Down" => (0, 1),
            "Right" => (1, 0),
            "Left" => (-1, 0),
            _ => (0, 0)
        };

        // Primitive operation 1: subclasses define which coordinates the weapon affects
        protected abstract List<(int x, int y)> SelectTargetTiles(Player player, Map map, int dx, int dy);

        // Primitive operation 2: subclasses define what happens on the selected coordinates
        protected abstract void ApplyWeaponPayload(Player player, Match match, Map map, List<(int x, int y)> targetTiles);

        // Hook: unequip weapon once depleted
        protected virtual void OnAfterUse(Player player)
        {
            if (UsesCount <= 0 && ReferenceEquals(player.CurrentWeapon, this))
            {
                player.CurrentWeapon = null;
            }
        }

        // Shared helpers for concrete weapon subclasses
        protected static List<(int x, int y)> TraceBeamTiles(Player player, Map map, int dx, int dy, int maxRange)
        {
            var targets = new List<(int x, int y)>();
            if (dx == 0 && dy == 0)
                return targets;

            int cx = player.X + dx;
            int cy = player.Y + dy;
            int range = 0;

            while (map.IsValidCoord(cx, cy) && range < maxRange)
            {
                var tile = map.GetTile(cx, cy);
                if (tile == null || tile.Type == "IndestructableTile")
                    break;

                targets.Add((cx, cy));
                if (tile is DestructableTile)
                    break;

                cx += dx;
                cy += dy;
                range++;
            }

            return targets;
        }

        protected static List<(int x, int y)> SelectThrowTarget(Player player, Map map, int dx, int dy, int distance)
        {
            int tx = player.X + (dx * distance);
            int ty = player.Y + (dy * distance);

            if (map.IsValidCoord(tx, ty) && map.CheckIfCanMove(tx, ty))
                return new List<(int x, int y)> { (tx, ty) };

            return new List<(int x, int y)>();
        }

        protected void ApplyEffectToTargets(Player player, Map map, List<(int x, int y)> targetTiles)
        {
            foreach (var (x, y) in targetTiles)
            {
                Effect.Apply(player, map, x, y);
            }
        }

        protected static void SpawnThrownBombs(Player player, Match match, List<(int x, int y)> targetTiles)
        {
            foreach (var (x, y) in targetTiles)
            {
                match.TrySpawnThrownBomb(player, x, y);
            }
        }

        public void Destroy()
        {
            // logic to remove from map
        }
    }

    // 1. Concrete sealed long-range fire laser weapon (6 tiles)
    public sealed class LaserGun : Weapon
    {
        private const int BeamRange = 6;

        public LaserGun(int x, int y, IWeaponEffect? effect = null) : base(x, y, 1, effect) { }

        public override string GetEmoji() => "🔫";

        protected override List<(int x, int y)> SelectTargetTiles(Player player, Map map, int dx, int dy) =>
            TraceBeamTiles(player, map, dx, dy, BeamRange);

        protected override void ApplyWeaponPayload(Player player, Match match, Map map, List<(int x, int y)> targetTiles) =>
            ApplyEffectToTargets(player, map, targetTiles);
    }

    // 2. Concrete sealed long-range frost laser weapon (3 tiles)
    public sealed class FrozenLaserGun : Weapon
    {
        private const int BeamRange = 3;

        public FrozenLaserGun(int x, int y, IWeaponEffect? effect = null)
            : base(x, y, 1, effect ?? new FrostFreezeEffect()) { }

        public override string GetEmoji() => "🍧";

        protected override List<(int x, int y)> SelectTargetTiles(Player player, Map map, int dx, int dy) =>
            TraceBeamTiles(player, map, dx, dy, BeamRange);

        protected override void ApplyWeaponPayload(Player player, Match match, Map map, List<(int x, int y)> targetTiles) =>
            ApplyEffectToTargets(player, map, targetTiles);
    }

    // 3. Concrete sealed mid-range throwable bomb weapon (2 tiles)
    public sealed class ThrowableWeapon : Weapon
    {
        private const int ThrowDistance = 2;

        public ThrowableWeapon(int x, int y, IWeaponEffect? effect = null) : base(x, y, 3, effect) { }

        public override string GetEmoji() => "🧨";

        protected override List<(int x, int y)> SelectTargetTiles(Player player, Map map, int dx, int dy) =>
            SelectThrowTarget(player, map, dx, dy, ThrowDistance);

        protected override void ApplyWeaponPayload(Player player, Match match, Map map, List<(int x, int y)> targetTiles) =>
            SpawnThrownBombs(player, match, targetTiles);
    }

    // 4. Concrete sealed frost throwable bomb weapon (2 tiles)
    public sealed class SnowballWeapon : Weapon
    {
        private const int ThrowDistance = 2;

        public SnowballWeapon(int x, int y, IWeaponEffect? effect = null)
            : base(x, y, 3, effect ?? new FrostFreezeEffect()) { }

        public override string GetEmoji() => "⚪";

        protected override List<(int x, int y)> SelectTargetTiles(Player player, Map map, int dx, int dy) =>
            SelectThrowTarget(player, map, dx, dy, ThrowDistance);

        protected override void ApplyWeaponPayload(Player player, Match match, Map map, List<(int x, int y)> targetTiles) =>
            SpawnThrownBombs(player, match, targetTiles);
    }

    // 5. Concrete sealed close-range melee weapon (adjacent tile)
    public sealed class Knife : Weapon
    {
        public Knife(int x, int y, IWeaponEffect? effect = null) : base(x, y, 3, effect) { }

        public override string GetEmoji() => "🔪";

        protected override List<(int x, int y)> SelectTargetTiles(Player player, Map map, int dx, int dy) =>
            new() { (player.X + dx, player.Y + dy) };

        protected override void ApplyWeaponPayload(Player player, Match match, Map map, List<(int x, int y)> targetTiles) =>
            ApplyEffectToTargets(player, map, targetTiles);
    }

    // 6. Concrete sealed close-range frost melee weapon (adjacent tile, 2 uses)
    public sealed class Popsicle : Weapon
    {
        public Popsicle(int x, int y, IWeaponEffect? effect = null)
            : base(x, y, 2, effect ?? new FrostFreezeEffect()) { }

        public override string GetEmoji() => "🍦";

        protected override List<(int x, int y)> SelectTargetTiles(Player player, Map map, int dx, int dy) =>
            new() { (player.X + dx, player.Y + dy) };

        protected override void ApplyWeaponPayload(Player player, Match match, Map map, List<(int x, int y)> targetTiles) =>
            ApplyEffectToTargets(player, map, targetTiles);
    }
}
