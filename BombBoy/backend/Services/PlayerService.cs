using BombBoy.Models;
using BombBoy.Models.Strategies;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;

namespace BombBoy.Services;

public sealed class PlayerService
{
    private readonly ConcurrentDictionary<string, Player> _players = new();

    public string FormatPlayerName(string connectionId) =>
        $"Player-{connectionId[..Math.Min(4, connectionId.Length)]}";

    public Player RegisterPlayer(string connectionId, string name, int[]? startPos)
    {
        var player = new Player
        {
            Id = connectionId,
            Name = name
        };

        if (startPos != null)
        {
            player.X = startPos[0];
            player.Y = startPos[1];
        }

        _players[connectionId] = player;
        return player;
    }

    public bool TryRemovePlayer(string connectionId, out Player? player) =>
        _players.TryRemove(connectionId, out player);

    public bool TryGetPlayer(string connectionId, out Player? player) =>
        _players.TryGetValue(connectionId, out player);

    public bool TryGetAlivePlayer(string connectionId, out Player? player) =>
        _players.TryGetValue(connectionId, out player) && player.IsAlive;

    public Player? GetPlayerOrDefault(string? connectionId) =>
        connectionId != null ? _players.GetValueOrDefault(connectionId) : null;

    public List<GameManager.PlayerDto> GetAllPlayers() =>
        _players.Values
            .Select(p => new GameManager.PlayerDto(p.Id, p.Name, p.X, p.Y, p.IsAlive))
            .ToList();

    public void SetPlayerStrategy(string connectionId, string strategyName)
    {
        if (_players.TryGetValue(connectionId, out var player))
        {
            player.ActiveBombStrategy = ResolveStrategy(strategyName);
        }
    }

    public IExplosionStrategy ResolveBombStrategy(Player player, string? strategyName) =>
        !string.IsNullOrWhiteSpace(strategyName)
            ? ResolveStrategy(strategyName)
            : player.ActiveBombStrategy;

    public static IExplosionStrategy ResolveStrategy(string? strategyName)
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
}
