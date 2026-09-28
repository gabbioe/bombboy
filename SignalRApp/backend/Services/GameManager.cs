using System.Collections.Concurrent;
using SignalRApp.Models;
using SignalRApp.Models.GridEntities;
using SignalRApp.Models.Strategies;

namespace SignalRApp.Services;

public class GameManager
{
    private readonly ConcurrentDictionary<string, Player> _players = new();
    private readonly List<Bomb> _activeBombs = new();
    private readonly object _lock = new();

    private readonly Map _map = new Map(11, 11, 4);

    public Player AddPlayer(string connectionId)
    {
        var player = new Player
        {
            Id = connectionId,
            Name = $"player-{connectionId[..4]}"
        };

        lock (_lock)
        {
            int[] startPos = _map.AddPlayer(player);
            player.X = startPos[0];
            player.Y = startPos[1];
        }

        _players[connectionId] = player;
        return player;
    }

    public bool RemovePlayer(string connectionId, out Player? player)
    {
        lock (_lock)
        {
            _map.RemovePlayer(connectionId);
            _activeBombs.RemoveAll(b => b.OwnerId == connectionId);
        }
        return _players.TryRemove(connectionId, out player);
    }

    public string GetMap()
    {
        lock (_lock)
        {
            return _map.ReturnMapToJSon();
        }
    }

    public void MovePlayer(string connectionId, int dx, int dy)
    {
        if (_players.TryGetValue(connectionId, out var player) && player.isAlive)
        {
            lock (_lock)
            {
                bool didMove = _map.Move(dx, dy, connectionId);

                if (didMove)
                {
                    player.X += dx;
                    player.Y += dy;
                }
            }
        }
    }

    public bool PlaceBomb(string connectionId, string? requestedStrategy = null)
    {
        if (!_players.TryGetValue(connectionId, out var player) || !player.isAlive)
            return false;

        lock (_lock)
        {
            int activeCount = _activeBombs.Count(b => b.OwnerId == connectionId && !b.HasExploded);
            if (activeCount >= player.BombCount)
                return false;

            int bx = (int)Math.Round(player.X);
            int by = (int)Math.Round(player.Y);

            var strategyType = requestedStrategy ?? player.BombStrategyType;
            var strategy = CreateStrategy(strategyType);

            var bomb = new Bomb(strategy, bx, by, range: player.Explosion, fuseTicks: 40, ownerId: connectionId);

            if (_map.TryPlaceBomb(bx, by, bomb))
            {
                _activeBombs.Add(bomb);
                return true;
            }
        }
        return false;
    }

    public void SetPlayerStrategy(string connectionId, string strategyName)
    {
        if (_players.TryGetValue(connectionId, out var player))
        {
            player.BombStrategyType = strategyName.ToLowerInvariant();
        }
    }

    public bool UpdateGameState()
    {
        bool mapChanged = false;

        lock (_lock)
        {
            // Update active bombs
            for (int i = _activeBombs.Count - 1; i >= 0; i--)
            {
                var bomb = _activeBombs[i];
                bomb.FuseTicks--;

                if (bomb.FuseTicks <= 0)
                {
                    bomb.Explode(_map);
                    _activeBombs.RemoveAt(i);
                    mapChanged = true;
                }
            }

            // Update active blasts
            if (_map.UpdateBlasts())
            {
                mapChanged = true;
            }
        }

        return mapChanged;
    }

    private static IExplosionStrategy CreateStrategy(string? strategyName)
    {
        return (strategyName?.ToLowerInvariant()) switch
        {
            "diagonal" => new DiagonalExplosionStrategy(),
            "area" or "cluster" => new AreaBoxExplosionStrategy(),
            "piercing" or "line" => new PiercingLineExplosionStrategy(),
            "floodfill" or "flood" => new FloodFillExplosionStrategy(),
            _ => new ClassicCrossExplosionStrategy()
        };
    }

    public IEnumerable<Player> GetAllPlayers() => _players.Values;
}