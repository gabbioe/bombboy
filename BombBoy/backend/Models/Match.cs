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
using System.Text.Json;
using System.Text.Json.Nodes;

namespace BombBoy.Models
{
    public class Match : IGameEventSubject
    {
        private readonly List<IGameEventObserver> _observers = new();

        private int PlayerCount = 0;
        private Map? _mapPrototype;
        private Map GameMap = null!;
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

        public Match(IAbstractItemFactory? itemFactory = null)
        {
            _itemFactory = itemFactory ?? new BasicItemFactory();
        }

        public string CurrentMapType { get; private set; } = "normal";

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

        private void NotifyObservers(Action<IGameEventObserver> notifyAction)
        {
            foreach (var obs in _observers.ToList())
            {
                notifyAction(obs);
            }
        }

        public void NotifyPlayerDied(Player victim, Player? killer) =>
            NotifyObservers(obs => obs.OnPlayerDied(victim, killer));

        public void NotifyRoundEnded(Player? winner, int roundNumber, int totalRounds) =>
            NotifyObservers(obs => obs.OnRoundEnded(winner, roundNumber, totalRounds));

        public void NotifyMatchEnded(Player? champion, List<PlayerStats> finalStandings) =>
            NotifyObservers(obs => obs.OnMatchEnded(champion, finalStandings));

        public void NotifyBombPlanted(Player player) =>
            NotifyObservers(obs => obs.OnBombPlanted(player));

        public void NotifyBombExploded(int x, int y) =>
            NotifyObservers(obs => obs.OnBombExploded(x, y));

        public void NotifyPlayerDamaged(Player victim, Player? attacker = null) =>
            NotifyObservers(obs => obs.OnPlayerDamaged(victim, attacker));

        public void NotifyPlayerDrowned(Player victim) =>
            NotifyObservers(obs => obs.OnPlayerDrowned(victim));

        public void NotifyItemSpawned(int x, int y) =>
            NotifyObservers(obs => obs.OnItemSpawned(x, y));

        public void NotifyItemPickedUp(Player player, string itemType) =>
            NotifyObservers(obs => obs.OnItemPickedUp(player, itemType));

        public void NotifyTileDestroyed(int x, int y) =>
            NotifyObservers(obs => obs.OnTileDestroyed(x, y));

        public void NotifyWeaponUsed(Player player, string weaponName) =>
            NotifyObservers(obs => obs.OnWeaponUsed(player, weaponName));

        public void NotifyRoundStarted(int roundNumber, string mapType) =>
            NotifyObservers(obs => obs.OnRoundStarted(roundNumber, mapType));

        public void NotifyPlayerMoved(Player player, string mapType) =>
            NotifyObservers(obs => obs.OnPlayerMoved(player, mapType));

        public void NotifyPlayerJoined(string playerName) =>
            NotifyObservers(obs => obs.OnPlayerJoined(playerName));

        public void NotifyPlayerLeft(string playerName) =>
            NotifyObservers(obs => obs.OnPlayerLeft(playerName));
        #endregion

        public bool ApplyDamage(Player victim, Player? killer)
        {
            if (!victim.IsAlive) return false;

            bool died = victim.TakeDamage();
            if (died)
            {
                NotifyPlayerDied(victim, killer);
            }
            else
            {
                NotifyPlayerDamaged(victim, killer);
            }
            return died;
        }

        public string GetMapStringFormat()
        {
            return GameMap.ReturnMapToConsole();
        }

        public void GenerateMap(string mapType = "normal")
        {
            CurrentMapType = mapType;
            IMapBuilder builder = new StandartSmallMapBuilder();
            if (mapType == "ice")
                builder = new WateryBigMapBuilder();

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
                    if (GameMap.IsValidCoord(x + dx, y + dy) && (player.CanPassThroughWalls || GameMap.CheckIfCanMove(x + dx, y + dy)))
                    {
                        player.X = x + dx;
                        player.Y = y + dy;
                        NotifyPlayerMoved(player, CurrentMapType);

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

                        // SpeedDecorator effect: move an additional step if speed bonus is active and next tile is clear
                        if (player.IsAlive && player.SpeedBonus > 0)
                        {
                            int nx = player.X + dx;
                            int ny = player.Y + dy;
                            if (GameMap.IsValidCoord(nx, ny) && (player.CanPassThroughWalls || GameMap.CheckIfCanMove(nx, ny)))
                            {
                                player.X = nx;
                                player.Y = ny;
                                CheckPickups(player);
                            }
                        }

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
            CurrentMapType = mapType;
            
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
                p.CanPassThroughWalls = false; // cheat
                p.ResetDecorators();
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
            AddEntityToGrid(bomb.X, bomb.Y, bomb);
            NotifyBombPlanted(player);
            return true;
        }

        // method to spawn items when a block is destroyed
        public void OnBlockDestroyed(int x, int y)
        {
            NotifyTileDestroyed(x, y);
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
                player.UpdateDecorators(deltaTime);
                if (player.IsAlive && GameMap.GetTile(player.X, player.Y) is WaterTile waterTile)
                {
                    if (waterTile.CanDrown())
                    {
                        NotifyPlayerDrowned(player);
                        player.Lives = 0;
                        player.IsAlive = false;
                        NotifyPlayerDied(player, null);
                    }
                }
                if(player.IsAlive && !player.CanPassThroughWalls && !GameMap.CheckIfCanMove(player.X, player.Y))
                {
                    player.HasShield = false;
                    player.Lives = 0;
                    player.IsAlive = false;
                    NotifyPlayerDied(player, null);
                }
            }            
            //update bombs
            for (int i = ActiveBombs.Count - 1; i >= 0; i--)
            {
                var bomb = ActiveBombs[i];
                if (bomb.Tick(deltaTime))
                {
                    bomb.Detonate(GameMap);
                    NotifyBombExploded(bomb.X, bomb.Y);
                    RemoveEntityFromGrid(bomb.X, bomb.Y, bomb);
                    ActiveBombs.RemoveAt(i);
                }
            }
            //update blasts
            for (int i = ActiveBlasts.Count - 1; i >= 0; i--)
            {
                var blast = ActiveBlasts[i];
                if (blast.Tick(deltaTime))
                {
                    RemoveEntityFromGrid(blast.X, blast.Y, blast);
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

            if (GameMap.GetTile(x, y)?.Type == "Tile" && !_items.Any(i => i.X == x && i.Y == y))
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
            NotifyItemSpawned(x, y);
        }

        // method to check for pickups
        public void CheckPickups(Player player)
        {
            var item = _items.FirstOrDefault(i => i.X == player.X && i.Y == player.Y);
            if (item != null)
            {
                item.Pickup(player);
                _items.Remove(item);
                NotifyItemPickedUp(player, item is Weapon ? "Weapon" : "Powerup");
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

        private void AddEntityToGrid(int x, int y, Entity entity)
        {
            if (!Entities.TryGetValue((x, y), out var list))
            {
                list = new List<Entity>();
                Entities[(x, y)] = list;
            }
            list.Add(entity);
        }

        private void RemoveEntityFromGrid(int x, int y, Entity entity)
        {
            if (Entities.TryGetValue((x, y), out var list))
            {
                list.Remove(entity);
                if (list.Count == 0)
                    Entities.Remove((x, y));
            }
        }

        public bool TrySpawnThrownBomb(Player player, int tx, int ty)
        {
            bool tileIsClear = GameMap.IsValidCoord(tx, ty)
                && GameMap.CheckIfCanMove(tx, ty)
                && !ActiveBombs.Any(b => b.X == tx && b.Y == ty);

            if (!tileIsClear)
                return false;

            var bomb = new Bomb(tx, ty, player.BombRange, player.ActiveBombStrategy, player.Id);
            ActiveBombs.Add(bomb);
            AddEntityToGrid(tx, ty, bomb);
            return true;
        }

        // weapon usage logic (delegates to Weapon Template Method)
        public void UseWeapon(Player player)
        {
            player.CurrentWeapon?.Use(player, this, GameMap);
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
                var blast = new Blast(bx, by, 0.5f, ownerId, map.CurrentBlastEmoji);
                AddEntityToGrid(bx, by, blast);
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
