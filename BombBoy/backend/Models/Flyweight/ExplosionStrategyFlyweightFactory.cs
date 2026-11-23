using BombBoy.Models.Strategies;
using System.Collections.Generic;

namespace BombBoy.Models.Flyweight;

public static class ExplosionStrategyFlyweightFactory
{
    private static readonly IExplosionStrategy ClassicFlyweight  = new ClassicCrossExplosionStrategy();
    private static readonly IExplosionStrategy DiagonalFlyweight = new DiagonalExplosionStrategy();
    private static readonly IExplosionStrategy AreaFlyweight     = new AreaBoxExplosionStrategy();
    private static readonly IExplosionStrategy PiercingFlyweight = new PiercingLineExplosionStrategy();
    private static readonly IExplosionStrategy FloodFlyweight    = new FloodFillExplosionStrategy();

    private static readonly IReadOnlyDictionary<string, IExplosionStrategy> Pool =
        new Dictionary<string, IExplosionStrategy>
        {
            ["classic"]  = ClassicFlyweight,
            ["diagonal"] = DiagonalFlyweight,
            ["area"]     = AreaFlyweight,
            ["piercing"] = PiercingFlyweight,
            ["flood"]    = FloodFlyweight
        };

    public static int CachedFlyweightCount => Pool.Count;

    public static IExplosionStrategy GetStrategy(string? strategyKey = null)
    {
        if (string.IsNullOrWhiteSpace(strategyKey))
            return ClassicFlyweight;

        if (Pool.TryGetValue(strategyKey, out var cached))
            return cached;

        string norm = strategyKey.Trim().ToLowerInvariant();
        if (norm.Contains("diag") || norm == "2") return DiagonalFlyweight;
        if (norm.Contains("area") || norm.Contains("box") || norm == "3") return AreaFlyweight;
        if (norm.Contains("pierc") || norm == "4") return PiercingFlyweight;
        if (norm.Contains("flood") || norm == "5") return FloodFlyweight;
        return ClassicFlyweight;
    }
}
