using SignalRApp.Models;
using SignalRApp.Models.TileFactories;
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

    public void GenerateMap()
    {
        _Match.GenerateMap(7,7,50,new BasicMapTileFactory());
        _Match.PrintMap();
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

            Console.WriteLine($"Player {player.Name} moved to ({player.X}, {player.Y}), player has moved:{didMove}");
        }
    }

    public IEnumerable<Player> GetAllPlayers() => _players.Values;
}