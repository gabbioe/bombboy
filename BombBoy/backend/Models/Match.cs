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
            List<JsonObject> EntityJsonArray = new List<JsonObject>();
            foreach(var e in Entities)
            {
                // just in case kitur nesortinta
                e.Value.Sort();
                var entry = new JsonObject
                {
                    //kad sutiktu su mapo lokacijomis
                    [$"{e.Key.x},{e.Key.y}"] = e.Value[0].ToString(),
                };
                EntityJsonArray.Add(entry);
            }

            // Serialize to a JSON string
            return JsonSerializer.Serialize(EntityJsonArray);
        }
        private string GetPlayerJson()
        {
            List<JsonObject> EntityJsonArray = new List<JsonObject>();
            foreach (var p in Players)
            {
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
