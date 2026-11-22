using BombBoy.Models.Flyweight;
using BombBoy.Models.GridEntities;
using BombBoy.Models.Strategies;
using System;
using System.Diagnostics;

namespace BombBoy.Benchmarks;

/// <summary>
/// Measures execution time and heap memory allocations for resolving and assigning
/// bomb explosion strategies WITHOUT Flyweight vs. WITH Flyweight.
/// </summary>
public static class FlyweightBenchmark
{
    private static readonly string[] StrategyKeys = { "classic", "diagonal", "area", "piercing", "flood" };

    // Pre-Flyweight implementation: allocates a new strategy instance every time
    private static IExplosionStrategy ResolveWithoutFlyweight(string? strategyName)
    {
        if (string.IsNullOrWhiteSpace(strategyName))
            return new ClassicCrossExplosionStrategy();

        string norm = strategyName.Trim().ToLowerInvariant();
        if (norm.Contains("diag") || norm == "2") return new DiagonalExplosionStrategy();
        if (norm.Contains("area") || norm.Contains("box") || norm == "3") return new AreaBoxExplosionStrategy();
        if (norm.Contains("pierc") || norm == "4") return new PiercingLineExplosionStrategy();
        if (norm.Contains("flood") || norm == "5") return new FloodFillExplosionStrategy();
        return new ClassicCrossExplosionStrategy();
    }

    // Current Flyweight implementation: reuses shared instances from ExplosionStrategyFlyweightFactory
    private static IExplosionStrategy ResolveWithFlyweight(string? strategyName) =>
        ExplosionStrategyFlyweightFactory.GetStrategy(strategyName);

    public static void Run(int iterations = 200_000)
    {
        Console.WriteLine("=================================================================");
        Console.WriteLine($" FLYWEIGHT BENCHMARK ({iterations:N0} strategy resolutions)");
        Console.WriteLine("=================================================================");

        // Warm-up JIT and Flyweight cache
        for (int i = 0; i < 100; i++)
        {
            _ = ResolveWithoutFlyweight(StrategyKeys[i % StrategyKeys.Length]);
            _ = ResolveWithFlyweight(StrategyKeys[i % StrategyKeys.Length]);
        }

        var strategiesWithout = new IExplosionStrategy[iterations];
        var strategiesWith = new IExplosionStrategy[iterations];

        // 1. Measure WITHOUT Flyweight
        GC.Collect();
        GC.WaitForPendingFinalizers();
        GC.Collect();

        long memBeforeWithout = GC.GetAllocatedBytesForCurrentThread();
        var swWithout = Stopwatch.StartNew();

        for (int i = 0; i < iterations; i++)
        {
            string key = StrategyKeys[i % StrategyKeys.Length];
            strategiesWithout[i] = ResolveWithoutFlyweight(key);
        }

        swWithout.Stop();
        long memAfterWithout = GC.GetAllocatedBytesForCurrentThread();
        long allocatedWithout = memAfterWithout - memBeforeWithout;

        // 2. Measure WITH Flyweight
        GC.Collect();
        GC.WaitForPendingFinalizers();
        GC.Collect();

        long memBeforeWith = GC.GetAllocatedBytesForCurrentThread();
        var swWith = Stopwatch.StartNew();

        for (int i = 0; i < iterations; i++)
        {
            string key = StrategyKeys[i % StrategyKeys.Length];
            strategiesWith[i] = ResolveWithFlyweight(key);
        }

        swWith.Stop();
        long memAfterWith = GC.GetAllocatedBytesForCurrentThread();
        long allocatedWith = memAfterWith - memBeforeWith;

        // Prevent dead-code elimination
        GC.KeepAlive(strategiesWithout);
        GC.KeepAlive(strategiesWith);

        double speedup = swWithout.Elapsed.TotalMilliseconds / Math.Max(0.001, swWith.Elapsed.TotalMilliseconds);

        Console.WriteLine($"Without Flyweight : {swWithout.Elapsed.TotalMilliseconds,8:F3} ms | {allocatedWithout,10:N0} bytes allocated ({iterations:N0} distinct strategy objects)");
        Console.WriteLine($"With Flyweight    : {swWith.Elapsed.TotalMilliseconds,8:F3} ms | {allocatedWith,10:N0} bytes allocated ({ExplosionStrategyFlyweightFactory.CachedFlyweightCount} shared flyweight objects)");
        Console.WriteLine($"Improvement       : {speedup,8:F2}x faster | {allocatedWithout - allocatedWith:N0} bytes saved");
        Console.WriteLine();
    }
}
