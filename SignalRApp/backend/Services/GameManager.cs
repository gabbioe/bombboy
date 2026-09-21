using System.Collections.Concurrent;

using SignalRApp.Models;

namespace SignalRApp.Services;

public class GameManager
{

 

    private readonly ConcurrentDictionary<string, Player> _players = new();

    public Player AddPlayer(string connectionId)
    {
        var player = new Player
        {
            Id = connectionId,
            Name = $"player-{connectionId[..4]}",
            X = 0,
            Y = 0
        };

        _players[connectionId] = player;
        return player;
    }

    public bool RemovePlayer(string connectionId, out Player? player)
    {
        return _players.TryRemove(connectionId, out player);
    }

    public void MovePlayer(string connectionId, float dx, float dy)
    {
        if(_players.TryGetValue(connectionId, out var player) && player.isAlive)
        {
            player.X += dx;
            player.Y += dy;
        }
    }

    public IEnumerable<Player> GetAllPlayers() => _players.Values;
}