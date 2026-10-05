using BombBoy.Models;
using BombBoy.Models.Strategies;
using BombBoy.Models.TileFactories;
using System.Collections.Concurrent;

namespace BombBoy.Services;

public class GameManager
{
    private readonly ConcurrentDictionary<string, Player> _players = new();

    private readonly Match _Match = new Match();

    public readonly IMapTileFactory _Factory = new FrozenMapTileFactory();


    //!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!
    //Tik kol negalime keisti per pati zaidima, veliau pakeist


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

    public void GenerateMap(int x, int y, int percentage)
    {
        _Match.GenerateMap(x,x, percentage, _Factory);
        _Match.GetMapStringFormat();
    }

    public bool RemovePlayer(string connectionId, out Player? player)
    {
        _Match.RemovePlayer(connectionId);
        return _players.TryRemove(connectionId, out player);
    }

    public string GetFullMap() => _Match.GetFullGameMapJson();

    public void MovePlayer(string connectionId, int dx, int dy)
    {
        if(_players.TryGetValue(connectionId, out var player) && player.IsAlive)
        {
            bool didMove = _Match.MovePlayer(dx, dy, connectionId);

            Console.WriteLine($"Player {player.Name} moved to ({player.X}, {player.Y}), player has moved:{didMove}");
        }
    }
    public void Update(float deltaTime)
    {
        _Match.Update(deltaTime);
    }
    public void PlaceBomb(string connectionId)
    {
        if (_players.TryGetValue(connectionId, out var player) && player.IsAlive)
        {
            _Match.PlaceBomb(player);
        }
    }

    public void SetPlayerStrategy(string connectionId, string strategyName)
    {
        if (_players.TryGetValue(connectionId, out var player))
        {
            player.ActiveBombStrategy = strategyName.ToLower() switch
            {
                "diagonal" => new DiagonalExplosionStrategy(),
                "area" => new AreaBoxExplosionStrategy(),
                "piercing" => new PiercingLineExplosionStrategy(),
                "floodfill" => new FloodFillExplosionStrategy(),
                _ => new ClassicCrossExplosionStrategy()
            };
        }
    }

    public IEnumerable<Player> GetAllPlayers() => _players.Values;
}