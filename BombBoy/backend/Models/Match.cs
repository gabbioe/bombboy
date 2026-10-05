using Microsoft.AspNetCore.Routing.Constraints;
using BombBoy.Models.GridEntities;
using BombBoy.Models.TileFactories;
using System;
using System.Collections.Generic;
using System.Reflection.Metadata.Ecma335;
using System.Text;
using System.Text.Json;
using System.Text.Json.Nodes;

namespace BombBoy.Models
{
    public class Match 
    {
        private int PlayerCount = 0;
        private Map GameMap;
        private Dictionary<(int x, int y), List<Entity>> Entities = new();

        //public bool MatchHasStarted;

        private int[][] StartingPositions = new int[4][];

        private List<int> AvailableStartingPositions = new List<int>{ 0, 1, 2, 3 };
        private List<Bomb> ActiveBombs = new();
        private List<Blast> ActiveBlasts = new();
        private Dictionary<string, Player> Players = new();
        public Match()
        {
            //GenerateMap(7, 7, 50, TileFactory);
            //MatchHasStarted = false;
        }
        
        public string GetMapStringFormat()
        {
            return GameMap.ReturnMapToConsole();
        }
        public void GenerateMap(int x, int y, int percentage, IMapTileFactory factory){

            //Console.WriteLine(factory.GetFactoryProductColours());
            GameMap = new Map(x, y, percentage, factory);
            GameMap.OnBlastSpawned += (bx, by, durationTicks) =>
            {
                var blast = new Blast(bx, by);
                if (!Entities.ContainsKey((bx, by)))
                    Entities[(bx, by)] = new List<Entity>();
                Entities[(bx, by)].Add(blast);
                ActiveBlasts.Add(blast);

                foreach (var p in Players.Values)
                {
                    if (p.IsAlive && p.X == bx && p.Y == by)
                    {
                        p.IsAlive = false;
                    }
                }

                var hitBomb = ActiveBombs.FirstOrDefault(b => b.X == bx && b.Y == by);
                if (hitBomb != null)
                {
                    hitBomb.DoDamage();
                }
            };
            //full map info
            StartingPositions[0] = [1, 1];
            StartingPositions[1] = [1, y ];
            StartingPositions[2] = [x, 1];
            StartingPositions[3] = [x, y];
            
            
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

                        Players[PlayerId].X = x+ dx;
                        Players[PlayerId].Y = y+ dy;

                        if (ActiveBlasts.Any(b => b.X == Players[PlayerId].X && b.Y == Players[PlayerId].Y))
                        {
                            Players[PlayerId].IsAlive = false;
                        }
                        switch (dx, dy)
                        {
                            case (0, 1):
                                Players[PlayerId].Direction = "Up";
                                break;
                            case (0, -1):
                                Players[PlayerId].Direction = "Down";
                                break;
                            case (1, 0):
                                Players[PlayerId].Direction = "right";
                                break;
                            case (-1, 0):
                                Players[PlayerId].Direction = "Left";
                                break;

                        }

                        return true;
                    }
                    return false;
                }
                throw new ArgumentException($"Player can only move in one direction at once, either dx({dx}) or dy({dy}) MUST be zero");
            }
            throw new ArgumentException($"Player with id {PlayerId} doesn't exist");
            
        }

        public int[] AddPlayer(Player newPlayer)
        {
            if (PlayerCount < 4 )
            {
                Players[newPlayer.Id] = newPlayer;
                int[] playerLocation = StartingPositions[AvailableStartingPositions[0]];
                Players[newPlayer.Id].StartingLocation = AvailableStartingPositions[0];
                AvailableStartingPositions.Remove(AvailableStartingPositions[0]);
                Players[newPlayer.Id].X = playerLocation[0];
                Players[newPlayer.Id].Y = playerLocation[1];
                PlayerCount++;
                return playerLocation;
            }

            throw new ArgumentException($"Game only supports up too 4 players");
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


        public bool PlaceBomb(Player player)
        {
            int playerCurrentBombs = ActiveBombs.Count(b => b.OwnerId == player.Id);
            if (playerCurrentBombs >= player.BombCount)
                return false;
            if (ActiveBombs.Any(b => b.X == player.X && b.Y == player.Y))
                return false;
            var bomb = new Bomb(player.X, player.Y, player.BombRange, player.ActiveBombStrategy, player.Id);
            ActiveBombs.Add(bomb);
            if (!Entities.ContainsKey((bomb.X, bomb.Y)))
                Entities[(bomb.X, bomb.Y)] = new List<Entity>();
            Entities[(bomb.X, bomb.Y)].Add(bomb);
            return true;
        }
        // Called every game tick
        public void Update(float deltaTime)
        {
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
        }
    
            //returns two jsons in one (first Bottom layer(tiles), then Top Layer(entities))
        public string GetFullGameMapJson(bool ToConsole = false)
        {
            var CombinedJson = new JsonObject
            {
                ["MapLayer"] = JsonNode.Parse(GameMap.ReturnMapToJSon()),
                ["EntityLayer"] = JsonNode.Parse(GetEntityJson()),
                ["PlayerLayer"] = JsonNode.Parse(GetPlayerJson())
            };

            var options = new JsonSerializerOptions
            {
                WriteIndented = ToConsole
            };

            return CombinedJson.ToJsonString(options);
            
        }

        //generates the Top Layer(entities)
        private string GetEntityJson()
        {
            var entityDict = new JsonObject();
            foreach(var e in Entities)
            {
                if (e.Value.Count > 0)
                {
                    // just in case kitur nesortinta
                    e.Value.Sort();
                    //kad sutiktu su mapo lokacijomis
                    entityDict[$"{e.Key.x},{e.Key.y}"] = e.Value[0].ToString();
                }
            }

            // Serialize to a JSON string
            return JsonSerializer.Serialize(entityDict);
        }
        private string GetPlayerJson()
        {
            List<JsonObject> EntityJsonArray = new List<JsonObject>();
            foreach (var p in Players)
            {
                if (!p.Value.IsAlive)
                    continue;

                var entry = new JsonObject
                {
                    //kad sutiktu su mapo lokacijomis
                    [$"{p.Value.X},{p.Value.Y}"] = p.Value.GetPossitionAndDirectionJson(),
                };
                EntityJsonArray.Add(entry);
            }

            // Serialize to a JSON string
            return JsonSerializer.Serialize(EntityJsonArray);
        }

    }
}
