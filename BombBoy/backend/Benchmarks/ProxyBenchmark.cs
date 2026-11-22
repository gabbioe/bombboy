using BombBoy.Services;
using BombBoy.Services.Proxy;
using System;
using System.Diagnostics;
using System.IO;

namespace BombBoy.Benchmarks;

/// <summary>
/// Measures execution time and heap memory allocations comparing:
///   1. Program WITH Flyweight only (direct GameManager)
///   2. Program WITH Flyweight + Proxy (GameManagerProtectionProxy wrapping GameManager)
/// </summary>
public static class ProxyBenchmark
{
    public static void Run(int cycles = 5_000)
    {
        Console.WriteLine("=================================================================");
        Console.WriteLine($" PROXY BENCHMARK ({cycles:N0} gameplay request/broadcast cycles)");
        Console.WriteLine("=================================================================");

        TextWriter originalOut = Console.Out;
        int directLen;
        int proxyLen;
        long allocatedDirect;
        long allocatedProxy;
        Stopwatch swDirect;
        Stopwatch swProxy;

        try
        {
            Console.SetOut(TextWriter.Null);

            // Warm-up
            RunWorkload(CreateDirectGameManager(), 50);
            RunWorkload(CreateProxiedGameManager(), 50);

            // 1. Program WITH Flyweight only (Direct GameManager)
            var directManager = CreateDirectGameManager();
            GC.Collect();
            GC.WaitForPendingFinalizers();
            GC.Collect();

            long memBeforeDirect = GC.GetAllocatedBytesForCurrentThread();
            swDirect = Stopwatch.StartNew();

            directLen = RunWorkload(directManager, cycles);

            swDirect.Stop();
            long memAfterDirect = GC.GetAllocatedBytesForCurrentThread();
            allocatedDirect = memAfterDirect - memBeforeDirect;

            // 2. Program WITH Flyweight + Proxy (GameManagerProtectionProxy)
            var proxiedManager = CreateProxiedGameManager();
            GC.Collect();
            GC.WaitForPendingFinalizers();
            GC.Collect();

            long memBeforeProxy = GC.GetAllocatedBytesForCurrentThread();
            swProxy = Stopwatch.StartNew();

            proxyLen = RunWorkload(proxiedManager, cycles);

            swProxy.Stop();
            long memAfterProxy = GC.GetAllocatedBytesForCurrentThread();
            allocatedProxy = memAfterProxy - memBeforeProxy;
        }
        finally
        {
            Console.SetOut(originalOut);
        }

        GC.KeepAlive(directLen);
        GC.KeepAlive(proxyLen);

        double speedup = swDirect.Elapsed.TotalMilliseconds / Math.Max(0.001, swProxy.Elapsed.TotalMilliseconds);
        double memReductionPct = allocatedDirect > 0
            ? (1.0 - ((double)allocatedProxy / allocatedDirect)) * 100.0
            : 0.0;

        Console.WriteLine($"Flyweight Only (Direct GameManager) : {swDirect.Elapsed.TotalMilliseconds,8:F3} ms | {allocatedDirect,12:N0} bytes allocated");
        Console.WriteLine($"Flyweight + Proxy (ProtectionProxy) : {swProxy.Elapsed.TotalMilliseconds,8:F3} ms | {allocatedProxy,12:N0} bytes allocated");
        Console.WriteLine($"Improvement with Proxy              : {speedup,8:F2}x faster | {memReductionPct,6:F1}% less memory ({allocatedDirect - allocatedProxy:N0} bytes saved)");
        Console.WriteLine();
    }

    private static IGameManager CreateDirectGameManager()
    {
        var gm = new GameManager();
        SetupMatch(gm);
        return gm;
    }

    private static IGameManager CreateProxiedGameManager()
    {
        var gm = new GameManager();
        var proxy = new GameManagerProtectionProxy(gm);
        SetupMatch(proxy);
        return proxy;
    }

    private static void SetupMatch(IGameManager manager)
    {
        manager.GenerateMap();
        manager.AddPlayer("host-p1");
        manager.AddPlayer("guest-p2");
        manager.ToggleReady("host-p1");
        manager.ToggleReady("guest-p2"); // Auto-starts match into GameStage.InGame
    }

    /// <summary>
    /// Simulates a mix of in-game client requests (including blocked/out-of-stage calls
    /// and strategy switches using Flyweight) followed by SignalR GetFullMap() state reads.
    /// </summary>
    private static int RunWorkload(IGameManager manager, int cycles)
    {
        int checksum = 0;
        for (int i = 0; i < cycles; i++)
        {
            // Periodically mutate state (every 10th cycle)
            if (i % 10 == 0)
            {
                manager.SetPlayerStrategy("host-p1", (i % 5 + 1).ToString());
                manager.MovePlayer("host-p1", (i % 2 == 0) ? 1 : -1, 0);
            }

            // Out-of-stage / unauthorized requests that the Protection Proxy short-circuits
            manager.SetMatchRounds("guest-p2", 9);
            manager.VoteMap("guest-p2", "ice");
            manager.VoteSameMap("guest-p2");

            // Broadcast state read
            string mapJson = manager.GetFullMap();
            checksum ^= mapJson.Length;
        }
        return checksum;
    }
}
