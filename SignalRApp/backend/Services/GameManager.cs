using SignalRApp.Models;
using System.Collections.Concurrent;

namespace SignalRApp.Services;

public class GameManager
{
    private readonly ConcurrentDictionary<string, Player> _players = new();

    private readonly Match _Match = new Match();

    public Player AddPlayer(string connectionId)
    {
        var player = new Player
        {
            Id = connectionId,
            Name = $"player-{connectionId[..4]}"
        };

        int[] startPos = _Match.AddPlayer(player);
        player.X = startPos[0];
        player.Y = startPos[1];

        _players[connectionId] = player;
        return player;
    }

    public bool RemovePlayer(string connectionId, out Player? player)
    {
        _Match.RemovePlayer(connectionId);
        return _players.TryRemove(connectionId, out player);
    }

    public string GetFullMap() => _Match.GetFullGameMapJson();

    public void MovePlayer(string connectionId, int dx, int dy)
    {
        if(_players.TryGetValue(connectionId, out var player) && player.isAlive)
        {
            bool didMove = _Match.Move(dx, dy, connectionId);

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