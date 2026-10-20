using BombBoy.Models;
using BombBoy.Models.Strategies;
using System.Collections.Concurrent;

namespace BombBoy.Services;

public class GameManager
{
    private readonly ConcurrentDictionary<string, Player> _players = new();
    private readonly object _lock = new();

    private readonly Match _Match = new Match();


    //!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!
    //Tik kol negalime keisti per pati zaidima, veliau pakeist


    public Player AddPlayer(string connectionId)
    {
        lock (_lock)
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
        
    }

    public void GenerateMap(int x, int y, int BreakableBlockSpawnChance, int waterTileSpawnChance)
    {
        lock (_lock)
        {
            _Match.GenerateMap();
            _Match.GetMapStringFormat();
        }
        
    }

    public bool RemovePlayer(string connectionId, out Player? player)
    {
        lock (_lock)
        {
            _Match.RemovePlayer(connectionId);
            return _players.TryRemove(connectionId, out player);
        }
        
    }

    public string GetFullMap()
    {
        lock (_lock)
        {
            return _Match.GetFullGameMapJson();
        }
        
    }

    public void MovePlayer(string connectionId, int dx, int dy)
    {
        lock (_lock)
        {
            if(_players.TryGetValue(connectionId, out var player) && player.IsAlive)
            {
                bool didMove = _Match.MovePlayer(dx, dy, connectionId);

                Console.WriteLine($"Player {player.Name} moved to ({player.X}, {player.Y}), player has moved:{didMove}");
            }
        }
        
    }
    public void Update(float deltaTime)
    {
        lock (_lock)
        {
            _Match.Update(deltaTime);
        }
        
    }
    public void PlaceBomb(string connectionId)
    {
        lock (_lock)
        {
            if (_players.TryGetValue(connectionId, out var player) && player.IsAlive)
            {
                _Match.PlaceBomb(player);
            }
        }        
    }

    public void SetPlayerStrategy(string connectionId, string strategyName)
    {
        lock (_lock)
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
        
    }

    public List<PlayerDto> GetAllPlayers()
    {
        lock (_lock)
        {
            return _players.Values
                .Select(p => new PlayerDto(p.Id, p.Name, p.X, p.Y, p.IsAlive))
                .ToList();
        }
    }

    public record PlayerDto(string Id, string Name, int X, int Y, bool IsAlive);

    public List<Match.ItemDto> GetAllItems() // use Match.ItemDto
    {
        lock (_lock)
        {
            return _Match.GetAllItems();
        }
    }

    public void UseWeapon(string connectionId)
    {
        lock (_lock)
        {
            if (_players.TryGetValue(connectionId, out var player) && player.IsAlive)
            {
                _Match.UseWeapon(player);
            }
        }
    }
}