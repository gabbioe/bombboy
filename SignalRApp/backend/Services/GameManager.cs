using System.Collections.Concurrent;

using SignalRApp.Models;

namespace SignalRApp.Services;

public class GameManager
{
    private readonly ConcurrentDictionary<string, Player> _players = new();

    private readonly Map _map = new Map(11, 11, 4);

    public Player AddPlayer(string connectionId)
    {
        var player = new Player
        {
            Id = connectionId,
            Name = $"player-{connectionId[..4]}"
        };

        int[] startPos = _map.AddPlayer(player);
        player.X = startPos[0];
        player.Y = startPos[1];

        _players[connectionId] = player;
        return player;
    }

    public bool RemovePlayer(string connectionId, out Player? player)
    {
        return _players.TryRemove(connectionId, out player);
    }

    public void MovePlayer(string connectionId, int dx, int dy)
    {
        if(_players.TryGetValue(connectionId, out var player) && player.isAlive)
        {
            bool didMove = _map.Move(dx, dy, connectionId);

            if (didMove)
            {
                player.X += dx;
                player.Y += dy;
            }
            
            Console.WriteLine($"Player {player.Name} moved to ({player.X}, {player.Y})");
        }
    }

    public IEnumerable<Player> GetAllPlayers() => _players.Values;
}