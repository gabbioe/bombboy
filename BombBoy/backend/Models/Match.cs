using Bombboy.Models.MapBuilders;
using Bombboy.Models.Tiles;
using BombBoy.Models.GridEntities;
using BombBoy.Models.ItemFactories;
using BombBoy.Models.Items;
using BombBoy.Models.Tiles;
using BombBoy.Models.Observer;
using BombBoy.Models.Strategies;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Text.Json.Nodes;

namespace BombBoy.Models
{
    public class Match : IGameEventSubject
    {
        private readonly List<IGameEventObserver> _observers = new();

        private int PlayerCount = 0;
        private Map? _mapPrototype;
        private Map GameMap;
        private Dictionary<(int x, int y), List<Entity>> Entities = new();

        private int[][] StartingPositions = new int[4][];
        private List<int> AvailableStartingPositions = new List<int> { 0, 1, 2, 3 };
        private List<Bomb> ActiveBombs = new();
        private List<Blast> ActiveBlasts = new();
        private Dictionary<string, Player> Players = new Dictionary<string, Player>();

        // for items (weapons and powerups):
        private List<IItem> _items = new List<IItem>();
        private IAbstractItemFactory _itemFactory;
        private Random _rand = new Random();

        public Match(IAbstractItemFactory itemFactory = null)
        {
            _itemFactory = itemFactory ?? new BasicItemFactory();
        }

        #region Observer Pattern Implementation
        public void Attach(IGameEventObserver observer)
        {
            if (!_observers.Contains(observer))
                _observers.Add(observer);
        }

        public void Detach(IGameEventObserver observer)
        {
            _observers.Remove(observer);
        }

        public void NotifyPlayerDied(Player victim, Player? killer)
        {
            foreach (var obs in _observers.ToList())
            {
                obs.OnPlayerDied(victim, killer);
            }
        }

        public void NotifyRoundEnded(Player? winner, int roundNumber, int totalRounds)
        {
            foreach (var obs in _observers.ToList())
            {
                obs.OnRoundEnded(winner, roundNumber, totalRounds);
            }
        }

        public void NotifyMatchEnded(Player? champion, List<PlayerStats> finalStandings)
        {
            foreach (var obs in _observers.ToList())
            {
                obs.OnMatchEnded(champion, finalStandings);
            }
        }
        #endregion

        public bool ApplyDamage(Player victim, Player? killer)
        {
            if (!victim.IsAlive) return false;

            bool died = victim.TakeDamage();
            if (died)
            {
                NotifyPlayerDied(victim, killer);
            }
            return died;
        }

        public string GetMapStringFormat()
        {
            return GameMap.ReturnMapToConsole();
        }

        public void GenerateMap(string mapType = "normal")
        {
            IMapBuilder builder = new StandartSmallMapBuilder();
            if (mapType == "ice")
                builder = new WateryBigMapBuilder();

            //builder.SetDimensions(7, 7).BuildBlockSpawnChance(20).BuildWaterSpawnChance(20).BuildHavePillars(false);

            GameMap = builder.GetResult();
            Console.WriteLine(GameMap.ReturnMapToJSon());
            _mapPrototype = GameMap.DeepCopy();
            // subscribe to Block Destroyed event to spawn items
            SubscribeMapevents(GameMap);

            StartingPositions[0] = new[] { 1, 1 };
            StartingPositions[1] = new[] { 1, GameMap.GetX() -2 };
            StartingPositions[2] = new[] { GameMap.GetX() - 2, 1 };
            StartingPositions[3] = new[] { GameMap.GetX() - 2, GameMap.GetY() - 2 };
            
        }

        /// <summary>
        /// moves the player on the map
        /// </summary>
        /// <param name="dx"> change in possition</param>
        /// <param name="dy"> change in possition</param>
        /// <param name="PlayerId"></param>
        /// <returns>True if player did move, False if player did not move</returns>
        public bool MovePlayer(int dx, int dy, string PlayerId)
        {
            
            if (Players.TryGetValue(PlayerId, out var player))
            {
                if (Math.Abs(dx) + Math.Abs(dy) < 2)
                {
                    int x = Players[PlayerId].X;
                    int y = Players[PlayerId].Y;
                    if (GameMap.CheckIfCanMove(x + dx, y + dy))
                    {
                        //Players[PlayerId].X = x+ dx;
                        //Players[PlayerId].Y = y+ dy;
                        player.X = x + dx;
                        player.Y = y + dy;

                        var blast = ActiveBlasts.FirstOrDefault(b => b.X == player.X && b.Y == player.Y);
                        if (blast != null)
                        {
                            Player? killer = null;
                            if (!string.IsNullOrEmpty(blast.OwnerId))
                            {
                                Players.TryGetValue(blast.OwnerId, out killer);
                            }
                            ApplyDamage(player, killer);
                        }
                        switch (dx, dy)
                        {
                            case (0, -1): player.Direction = "Up"; break;
                            case (0, 1): player.Direction = "Down"; break;
                            case (1, 0): player.Direction = "Right"; break;
                            case (-1, 0): player.Direction = "Left"; break;
                        }
                        // check for item pickup
                        CheckPickups(player);
                        return true;


                    }
                    return false;
                }
                throw new ArgumentException($"Player can only move in one direction at once, either dx({dx}) or dy({dy}) MUST be zero");
            }
            throw new ArgumentException($"Player with id {PlayerId} doesn't exist");            
        }

        public void ResetForNewRound(string mapType = "normal", bool useSameMap = false)
        {
            ActiveBombs.Clear();
            ActiveBlasts.Clear();
            _items.Clear();
            Entities.Clear();
            _itemSpawnTimer = 0f;

            if (mapType.ToLower() == "ice")
            {
                _itemFactory = new FrozenItemFactory();
            }
            else
            {
                _itemFactory = new BasicItemFactory();
            }

            if (useSameMap && _mapPrototype != null)
            {
                GameMap = _mapPrototype.DeepCopy();
                SubscribeMapevents(GameMap);
            }
            else
            {
                GenerateMap(mapType);
            }

            AvailableStartingPositions = new List<int> { 0, 1, 2, 3 };
            foreach (var p in Players.Values)
            {
                p.IsAlive = true;
                p.Lives = 1;
                p.HasShield = false;
                p.ShieldDuration = 0;
                p.CurrentWeapon = null;
                p.BombCount = 1;
                p.BombRange = 1;
                if (AvailableStartingPositions.Count > 0)
                {
                    p.StartingLocation = AvailableStartingPositions[0];
                    AvailableStartingPositions.RemoveAt(0);
                    int[] loc = StartingPositions[p.StartingLocation];
                    p.X = loc[0];
                    p.Y = loc[1];
                }
            }
        }

        public List<Player> GetAlivePlayers()
        {
            return Players.Values.Where(p => p.IsAlive).ToList();
        }

        public List<Player> GetAllPlayersList()
        {
            return Players.Values.ToList();
        }

        public int[]? AddPlayer(Player newPlayer)
        {
            if (PlayerCount < 4 && AvailableStartingPositions.Count > 0)
            {
                Players[newPlayer.Id] = newPlayer;
                int locIndex = AvailableStartingPositions[0];
                int[] playerLocation = StartingPositions[locIndex];
                newPlayer.StartingLocation = locIndex;
                AvailableStartingPositions.RemoveAt(0);
                newPlayer.X = playerLocation[0];
                newPlayer.Y = playerLocation[1];
                PlayerCount++;
                return playerLocation;
            }

            return null;
        }
        
        public bool RemovePlayer(string playerId)
        {
            if (Players.TryGetValue(playerId, out var player))
            {
                Players.Remove(playerId);
                AvailableStartingPositions.Add(player.StartingLocation);
                PlayerCount = Math.Max(0, PlayerCount - 1);
                return true;
            }
            return false;
        }

        public bool AreAllPlayersDead()
        {
            return Players.Count > 0 && Players.Values.All(p => !p.IsAlive);
        }

        public bool PlaceBomb(Player player, IExplosionStrategy? customStrategy = null)
        {
            int playerCurrentBombs = ActiveBombs.Count(b => b.OwnerId == player.Id);
            if (playerCurrentBombs >= player.BombCount)
                return false;
            if (ActiveBombs.Any(b => b.X == player.X && b.Y == player.Y))
                return false;

            var strategy = customStrategy ?? player.ActiveBombStrategy ?? new ClassicCrossExplosionStrategy();
            player.ActiveBombStrategy = strategy;
            var bomb = new Bomb(player.X, player.Y, player.BombRange, strategy, player.Id);
            ActiveBombs.Add(bomb);
            if (!Entities.ContainsKey((bomb.X, bomb.Y)))
                Entities[(bomb.X, bomb.Y)] = new List<Entity>();
            Entities[(bomb.X, bomb.Y)].Add(bomb);
            return true;
        }

        // method to spawn items when a block is destroyed
        public void OnBlockDestroyed(int x, int y)
        {
            // 20% chance to spawn an item
            if (_rand.Next(0, 100) < 20)
            {
                SpawnRandomItem(x, y);
            }
        }

        // method to spawn items periodically (called in Update)
        private float _itemSpawnTimer = 0f;
        private float _itemSpawnInterval = 10f; // every 10 seconds

        // Called every game tick
        public void Update(float deltaTime)
        {
            // update shields (tbh idk ar prieš ar po active bombs tickerių dėti logiką)
            foreach (var player in Players.Values)
            {
                if (player.HasShield)
                {
                    player.ShieldDuration -= (int)deltaTime;
                    if (player.ShieldDuration <= 0)
                    {
                        player.HasShield = false;
                    }
                }
                if (GameMap.GetTile(player.X, player.Y) is WaterTile waterTile)
                {
                    if (waterTile.CanDrown())
                        ApplyDamage(player, null);
                }
            }            
            //update bombs
            for (int i = ActiveBombs.Count - 1; i >= 0; i--)
            {
                var bomb = ActiveBombs[i];
                if (bomb.Tick(deltaTime))
                {
                    bomb.Detonate(GameMap);
                    if (Entities.TryGetValue((bomb.X, bomb.Y), out var list))
                    {
                        list.Remove(bomb);
                        if (list.Count == 0) Entities.Remove((bomb.X, bomb.Y));
                    }
                    ActiveBombs.RemoveAt(i);
                }
            }
            //update blasts
            for (int i = ActiveBlasts.Count - 1; i >= 0; i--)
            {
                var blast = ActiveBlasts[i];
                if (blast.Tick(deltaTime))
                {
                    if (Entities.TryGetValue((blast.X, blast.Y), out var list))
                    {
                        list.Remove(blast);
                        if (list.Count == 0) Entities.Remove((blast.X, blast.Y));
                    }
                    ActiveBlasts.RemoveAt(i);
                }
            }

            // item spawn timer
            _itemSpawnTimer += deltaTime;
            if (_itemSpawnTimer >= _itemSpawnInterval)
            {
                _itemSpawnTimer = 0f;
                TrySpawnRandomItemOnFloor();
            }
        }

        private void TrySpawnRandomItemOnFloor()
        {
            // find a random floor tile
            int x = _rand.Next(1, GameMap.GetX() - 1);
            int y = _rand.Next(1, GameMap.GetY() - 1);

            if (GameMap.GetTile(x, y).Type == "Tile" && !_items.Any(i => i.X == x && i.Y == y))
            {
                SpawnRandomItem(x, y);
            }
        }

        private void SpawnRandomItem(int x, int y)
        {
            IItem newItem;
            if (_rand.Next(0, 2) == 0)
            {
                newItem = _itemFactory.CreatePowerup(x, y);
            }
            else
            {
                newItem = _itemFactory.CreateWeapon(x, y);
            }
            _items.Add(newItem);
        }

        // method to check for pickups
        public void CheckPickups(Player player)
        {
            var item = _items.FirstOrDefault(i => i.X == player.X && i.Y == player.Y);
            if (item != null)
            {
                item.Pickup(player);
                _items.Remove(item);
            }
        }

        // method to handle item destruction by blasts
        public void DestroyItemsInBlast(int x, int y)
        {
            var itemsToDestroy = _items.Where(i => i.X == x && i.Y == y).ToList();
            foreach (var item in itemsToDestroy)
            {
                item.Destroy();
                _items.Remove(item);
            }
        }
                
        public List<ItemDto> GetAllItems()
        {
            return _items.Select(i => new ItemDto(i.X, i.Y, i.GetEmoji())).ToList();
        }

        /// <summary>
        /// data transfer object for items
        /// </summary>
        /// <param name="X"></param>
        /// <param name="Y"></param>
        /// <param name="Emoji"></param>
        public record ItemDto(int X, int Y, string Emoji);

        // weapon usage logic
        public void UseWeapon(Player player)
        {
            if (player.CurrentWeapon == null) return;

            var weapon = player.CurrentWeapon;
            weapon.Use(player); // decrements uses internally

            if (weapon is Knife || weapon is Popsicle)
            {
                // logic for knife
                int targetX = player.X;
                int targetY = player.Y;
                switch (player.Direction)
                {
                    case "Up": targetY -= 1; break;
                    case "Down": targetY += 1; break;
                    case "Right": targetX += 1; break;
                    case "Left": targetX -= 1; break;
                }
                // check if target is a player
                var targetPlayer = Players.Values.FirstOrDefault(p => p.X == targetX && p.Y == targetY && p.IsAlive);
                if (targetPlayer != null)
                {
                    ApplyDamage(targetPlayer, player);
                }
                else
                {
                    // check if target is a destructible tile
                    var tile = GameMap.GetTile(targetX, targetY);
                    if (tile != null && tile.DoDamage())
                    {
                        GameMap.DestroyTile(targetX, targetY);
                    }
                }
            }
            else if (weapon is LaserGun || weapon is FrozenLaserGun)
            {
                // logic for laser gun
                int dx = 0, dy = 0;
                switch (player.Direction)
                {
                    case "Up": dy = -1; break;
                    case "Down": dy = 1; break;
                    case "Right": dx = 1; break;
                    case "Left": dx = -1; break;
                }

                int cx = player.X + dx;
                int cy = player.Y + dy;
                int range = 0;
                int maxRange = (weapon is FrozenLaserGun) ? 3 : 6; // Frozen has half range

                GameMap.CurrentBlastOwnerId = player.Id;
                while (GameMap.IsValidCoord(cx, cy) && range < maxRange)
                {
                    var tile = GameMap.GetTile(cx, cy);
                    if (tile.Type == "IndestructableTile") break;

                    GameMap.SpawnBlast(cx, cy, 5);
                    if (tile is DestructableTile) break;

                    cx += dx;
                    cy += dy;
                    range++;
                }
                GameMap.CurrentBlastOwnerId = null;
            }
            else if (weapon is ThrowableWeapon || weapon is SnowballWeapon)
            {
                // logic for throwable weapon
                int dx = 0, dy = 0;
                switch (player.Direction)
                {
                    case "Up": dy = -1; break;
                    case "Down": dy = 1; break;
                    case "Right": dx = 1; break;
                    case "Left": dx = -1; break;
                }
                
                // target 2 blocks away
                int tx = player.X + (dx * 2);
                int ty = player.Y + (dy * 2);

                bool tileIsClear = GameMap.IsValidCoord(tx, ty)
                   && GameMap.CheckIfCanMove(tx, ty)  // not a wall or breakable block
                   && !ActiveBombs.Any(b => b.X == tx && b.Y == ty);
                   //&& !Players.Values.Any(p => p.X == tx && p.Y == ty && p.IsAlive); // juokingiau jei gali ant žmonių bombas mest tai nepridedu logikos

                if (tileIsClear)
                {
                    // spawn a bomb there owned by the player
                    var bomb = new Bomb(tx, ty, player.BombRange, player.ActiveBombStrategy, player.Id);
                    ActiveBombs.Add(bomb);
                    if (!Entities.ContainsKey((tx, ty)))
                        Entities[(tx, ty)] = new List<Entity>();
                    Entities[(tx, ty)].Add(bomb);
                }
            }
            // remove weapon if out of uses
            if (player.CurrentWeapon.UsesCount <= 0)
            {
                player.CurrentWeapon = null;
            }
        }

        //returns two jsons in one (first Bottom layer(tiles), then Top Layer(entities))
        public string GetFullGameMapJson(bool ToConsole = false)
        {
            var CombinedJson = new JsonObject
            {
                ["MapLayer"] = JsonNode.Parse(GameMap.ReturnMapToJSon()),
                ["EntityLayer"] = JsonNode.Parse(GetEntityJson()),
                ["PlayerLayer"] = JsonNode.Parse(GetPlayerJson()),
                ["AllPlayersDead"] = AreAllPlayersDead(),
                ["PlayerCount"] = Players.Count
            };

            var options = new JsonSerializerOptions { WriteIndented = ToConsole };
            return CombinedJson.ToJsonString(options);
        }

        //generates the Top Layer(entities & items)
        private string GetEntityJson()
        {
            var entityDict = new JsonObject();

            foreach (var e in Entities) // bombs and blasts
            {
                if (e.Value.Count > 0)
                {
                    // just in case kitur nesortinta
                    e.Value.Sort();
                    //kad sutiktu su mapo lokacijomis
                    entityDict[$"{e.Key.x},{e.Key.y}"] = e.Value[0].ToString();
                }
            }

            // add items to the entity layer
            foreach (var item in _items)
            {
                entityDict[$"{item.X},{item.Y}"] = item.GetEmoji();
            }

            return JsonSerializer.Serialize(entityDict);
        }





        private string GetPlayerJson()
        {
            List<JsonObject> EntityJsonArray = new List<JsonObject>();
            foreach (var p in Players)
            {
                if (!p.Value.IsAlive) continue;

                var entry = new JsonObject
                {
                    //kad sutiktu su mapo lokacijomis
                    [$"{p.Value.X},{p.Value.Y}"] = p.Value.GetPossitionAndDirectionJson(),
                };
                EntityJsonArray.Add(entry);
            }
            return JsonSerializer.Serialize(EntityJsonArray);
        }

        private void SubscribeMapevents(Map map)
        {
            map.OnBlockDestroyed += OnBlockDestroyed;
            map.OnBlastSpawnedWithOwner += (bx, by, durationTicks, ownerId) =>
            {
                var blast = new Blast(bx, by, 0.5f, ownerId);
                if (!Entities.ContainsKey((bx, by)))
                    Entities[(bx, by)] = new List<Entity>();
                Entities[(bx, by)].Add(blast);
                ActiveBlasts.Add(blast);

                // destroy items in blast
                DestroyItemsInBlast(bx, by);

                foreach (var p in Players.Values)
                {
                    if (p.IsAlive && p.X == bx && p.Y == by)
                    {
                        Player? killer = null;
                        if (!string.IsNullOrEmpty(blast.OwnerId))
                        {
                            Players.TryGetValue(blast.OwnerId, out killer);
                        }
                        ApplyDamage(p, killer);
                    }
                }

                var hitBomb = ActiveBombs.FirstOrDefault(b => b.X == bx && b.Y == by);
                if (hitBomb != null)
                {
                    hitBomb.DoDamage();
                }
            };
        }

    }
}
