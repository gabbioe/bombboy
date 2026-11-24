using BombBoy.Models.Flyweight;
using BombBoy.Models.GridEntities;
using BombBoy.Models.Strategies;
using System;
using System.Diagnostics;

namespace BombBoy.Benchmarks;

public static class FlyweightBenchmark
{
    private static readonly string[] StrategyKeys = { "classic", "diagonal", "area", "piercing", "flood" };

    
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

    
    private static IExplosionStrategy ResolveWithFlyweight(string? strategyName) =>
        ExplosionStrategyFlyweightFactory.GetStrategy(strategyName);

    public static void Run(int iterations = 200_000)
    {
        
        Console.WriteLine($"flyweight speed test ({iterations:N0} strategy resolutions)");
        

        
        for (int i = 0; i < 100; i++)
        {
            _ = ResolveWithoutFlyweight(StrategyKeys[i % StrategyKeys.Length]);
            _ = ResolveWithFlyweight(StrategyKeys[i % StrategyKeys.Length]);
        }

        var strategiesWithout = new IExplosionStrategy[iterations];
        var strategiesWith = new IExplosionStrategy[iterations];

        // 1. without flyweight
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

        // 2. with flyweight
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

        // dont make the gc delete these objs
        GC.KeepAlive(strategiesWithout);
        GC.KeepAlive(strategiesWith);

        double speedup = swWithout.Elapsed.TotalMilliseconds / Math.Max(0.001, swWith.Elapsed.TotalMilliseconds);

        Console.WriteLine($"without flyweight : {swWithout.Elapsed.TotalMilliseconds,8:F3} ms | {allocatedWithout,10:N0} bytes allocated ({iterations:N0} unique)");
        Console.WriteLine($"with flyweight : {swWith.Elapsed.TotalMilliseconds,8:F3} ms | {allocatedWith,10:N0} bytes allocated ({ExplosionStrategyFlyweightFactory.CachedFlyweightCount} shared)");
        Console.WriteLine($"improved : {speedup,8:F2}x faster | {allocatedWithout - allocatedWith:N0} bytes saved");
        Console.WriteLine();
    }
}
