using BombBoy.Models.Flyweight;
using BombBoy.Models.Strategies;

namespace BombBoy.Models.Composite;

/// <summary>
/// Builds visually and geometrically distinct multi-layered Composite explosion trees
/// and handles dynamic Bomb Fusion upgrades.
/// </summary>
public static class CompositeBombPresets
{
    /// <summary>
    /// Option 6 (Depth-2 Composite): Bullseye Crosshair.
    /// Fiery center dot + empty 3x3 safe moat + hollow 5x5 purple Void ring + 4 outer cyan sniper sight-ticks.
    /// </summary>
    public static CompositeExplosionStrategy CreatePlasmaStarburst()
    {
        var sniperTicks = new CompositeExplosionStrategy("SniperTicks", bombEmoji: "⚛️");
        sniperTicks.Add(new OffsetSpotLeaf(0, -3, "TickN", "⚡"));
        sniperTicks.Add(new OffsetSpotLeaf(0, 3, "TickS", "⚡"));
        sniperTicks.Add(new OffsetSpotLeaf(-3, 0, "TickW", "⚡"));
        sniperTicks.Add(new OffsetSpotLeaf(3, 0, "TickE", "⚡"));

        var bullseye = new CompositeExplosionStrategy("Bullseye", bombEmoji: "⚛️");
        bullseye.Add(new OffsetSpotLeaf(0, 0, "CenterPin", "💥"));
        bullseye.Add(new HollowRingLeaf("VoidRing", blastEmoji: "🔮", baseRadius: 2));
        bullseye.Add(sniperTicks);
        return bullseye;
    }

    /// <summary>
    /// Option 7 (Depth-2 Nested Composite): Grandmaster Crown.
    /// Short purple '+' cross + nested OrbitalCrown (8 Chess-Knight L-jumps curving around pillars + icy diagonal spikes).
    /// </summary>
    public static CompositeExplosionStrategy CreateArcaneCataclysm()
    {
        var orbitalCrown = new CompositeExplosionStrategy("OrbitalCrown", bombEmoji: "🌌");
        orbitalCrown.Add(new KnightScatterLeaf("KnightJumps", blastEmoji: "⚡"));
        orbitalCrown.Add(new ExplosionLeaf(
            ExplosionStrategyFlyweightFactory.GetStrategy("diagonal"),
            label: "FrostSpikes",
            blastEmoji: "❄️",
            rangeBonus: 2));

        var crown = new CompositeExplosionStrategy("Grandmaster", bombEmoji: "🌌");
        crown.Add(new ExplosionLeaf(
            ExplosionStrategyFlyweightFactory.GetStrategy("classic"),
            label: "VoidCross",
            blastEmoji: "🔮",
            rangeBonus: 0));
        crown.Add(orbitalCrown);
        return crown;
    }

    /// <summary>
    /// Option 8 (Depth-2 Nested Composite): Bio-Checkerboard.
    /// Alternating 5x5 neon-green chessboard grid (safe pockets on odd tiles) + 4 fiery corner pins.
    /// </summary>
    public static CompositeExplosionStrategy CreateBioCluster()
    {
        var cornerPins = new CompositeExplosionStrategy("CornerPins", bombEmoji: "☣️");
        cornerPins.Add(new OffsetSpotLeaf(-3, -3, "PinNW", "💥"));
        cornerPins.Add(new OffsetSpotLeaf(3, -3, "PinNE", "💥"));
        cornerPins.Add(new OffsetSpotLeaf(-3, 3, "PinSW", "💥"));
        cornerPins.Add(new OffsetSpotLeaf(3, 3, "PinSE", "💥"));

        var checker = new CompositeExplosionStrategy("BioChecker", bombEmoji: "☣️");
        checker.Add(new CheckerboardLeaf("ToxicGrid", blastEmoji: "☢️", baseRadius: 2, parity: 0));
        checker.Add(cornerPins);
        return checker;
    }

    /// <summary>
    /// Option 9 (Depth-2 Nested Composite): Frostfire Double-Ring.
    /// Inner 3x3 fire ring + empty safe ring at radius 2 + outer 7x7 freezing frost ring, bridged by cyan piercing beams.
    /// </summary>
    public static CompositeExplosionStrategy CreateFrostfireVortex()
    {
        var concentricRings = new CompositeExplosionStrategy("DualRings", bombEmoji: "☯️");
        concentricRings.Add(new HollowRingLeaf("InnerFire", blastEmoji: "💥", baseRadius: 1, scaleWithRange: false));
        concentricRings.Add(new HollowRingLeaf("OuterFrost", blastEmoji: "❄️", baseRadius: 3, scaleWithRange: true));

        var vortex = new CompositeExplosionStrategy("DoubleRing", bombEmoji: "☯️");
        vortex.Add(concentricRings);
        vortex.Add(new ExplosionLeaf(
            ExplosionStrategyFlyweightFactory.GetStrategy("piercing"),
            label: "PlasmaBridge",
            blastEmoji: "⚡",
            rangeBonus: 2));
        return vortex;
    }

    /// <summary>
    /// Non-mutating fusion used by BombFusionPowerup: wraps the player's current strategy and adds a new
    /// geometric Leaf shape (HollowRing, KnightStar, Checkerboard, Diagonal, or Piercing).
    /// </summary>
    public static CompositeExplosionStrategy Fuse(IExplosionStrategy current, string fusionKey)
    {
        IExplosionComponent existingNode = current is IExplosionComponent comp
            ? comp
            : new ExplosionLeaf(current, IExplosionComponent.Describe(current), blastEmoji: "💥", rangeBonus: 0);

        IExplosionComponent newShape = fusionKey switch
        {
            "ring"     => new HollowRingLeaf("VoidRing", blastEmoji: "🔮", baseRadius: 2),
            "knight"   => new KnightScatterLeaf("KnightStar", blastEmoji: "⚡"),
            "checker"  => new CheckerboardLeaf("BioChecker", blastEmoji: "☢️", baseRadius: 2, parity: 0),
            "diagonal" => new ExplosionLeaf(ExplosionStrategyFlyweightFactory.GetStrategy("diagonal"), "FrostX", "❄️", rangeBonus: 2),
            _          => new ExplosionLeaf(ExplosionStrategyFlyweightFactory.GetStrategy("piercing"), "PlasmaLance", "⚡", rangeBonus: 2)
        };

        var fused = new CompositeExplosionStrategy("Fused", bombEmoji: "⚛️");
        fused.Add(existingNode);
        fused.Add(newShape);
        return fused;
    }
}
