using BombBoy.Models;
using BombBoy.Models.Observer;
using BombBoy.Models.Strategies;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Text.Json.Nodes;

namespace BombBoy.Services;

public enum GameStage
{
    Lobby,
    InGame,
    RoundEnd,
    MatchEnd
}

public class LobbyPlayer
{
    public string Id { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public bool IsHost { get; set; } = false;
    public bool IsReady { get; set; } = false;
    public string MapVote { get; set; } = "normal"; // "normal" or "ice"
}

public class GameManager
{
    private readonly ConcurrentDictionary<string, Player> _players = new();
    private readonly ConcurrentDictionary<string, LobbyPlayer> _lobbyPlayers = new();
    private readonly object _lock = new();
    private readonly HashSet<string> _sameVotes = new();

    private readonly Match _Match = new Match();
    private readonly LeaderboardObserver _leaderboard = new();
    private readonly GameEventLoggerObserver _logger = new();

    private GameStage _stage = GameStage.Lobby;

    public int TotalRounds { get; set; } = 5;
    public int RoundDurationSeconds { get; set; } = 180;
    public int CurrentRound { get; private set; } = 1;
    public float RemainingSeconds { get; private set; } = 180f;
    public float IntermissionCountdown { get; private set; } = 0f;
    public string SelectedMapType { get; private set; } = "normal";
    public string RoundWinnerName { get; private set; } = string.Empty;
    public string ChampionName { get; private set; } = string.Empty;

    public GameManager()
    {
        _Match.Attach(_leaderboard);
        _Match.Attach(_logger);
    }

    public (Player? player, bool isFull) AddPlayer(string connectionId)
    {
        lock (_lock)
        {
            if (_lobbyPlayers.Count >= 4)
            {
                return (null, true);
            }

            bool isHost = _lobbyPlayers.Count == 0;
            string name = $"Player-{connectionId[..Math.Min(4, connectionId.Length)]}";

            var lobbyPlayer = new LobbyPlayer
            {
                Id = connectionId,
                Name = name,
                IsHost = isHost,
                IsReady = false,
                MapVote = "normal"
            };
            _lobbyPlayers[connectionId] = lobbyPlayer;
            _leaderboard.RegisterPlayer(connectionId, name);

            var player = new Player
            {
                Id = connectionId,
                Name = name
            };

            int[]? startPos = _Match.AddPlayer(player);
            if (startPos != null)
            {
                player.X = startPos[0];
                player.Y = startPos[1];
            }

            _players[connectionId] = player;
            return (player, false);
        }
    }

    public void GenerateMap()
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
            _lobbyPlayers.TryRemove(connectionId, out var removedLobby);
            _Match.RemovePlayer(connectionId);
            bool removed = _players.TryRemove(connectionId, out player);

            if (removedLobby != null && removedLobby.IsHost && _lobbyPlayers.Count > 0)
            {
                var newHost = _lobbyPlayers.Values.First();
                newHost.IsHost = true;
            }

            if (_lobbyPlayers.Count == 0)
            {
                _stage = GameStage.Lobby;
                _leaderboard.Clear();
                CurrentRound = 1;
            }

            return removed;
        }
    }

    public void ToggleReady(string connectionId)
    {
        lock (_lock)
        {
            if (_stage != GameStage.Lobby) return;

            if (_lobbyPlayers.TryGetValue(connectionId, out var lp))
            {
                lp.IsReady = !lp.IsReady;
                CheckAutoStart();
            }
        }
    }

    public void VoteMap(string connectionId, string mapType)
    {
        lock (_lock)
        {
            if (_stage != GameStage.Lobby) return;

            if (_lobbyPlayers.TryGetValue(connectionId, out var lp))
            {
                lp.MapVote = mapType.ToLower() == "ice" ? "ice" : "normal";
            }
        }
    }

    public bool SetMatchRounds(string connectionId, int rounds)
    {
        lock (_lock)
        {
            if (_stage != GameStage.Lobby) return false;
            if (!_lobbyPlayers.TryGetValue(connectionId, out var lp) || !lp.IsHost) return false;

            TotalRounds = Math.Clamp(rounds, 1, 10);
            return true;
        }
    }

    public bool SetMatchDuration(string connectionId, int durationSeconds)
    {
        lock (_lock)
        {
            if (_stage != GameStage.Lobby) return false;
            if (!_lobbyPlayers.TryGetValue(connectionId, out var lp) || !lp.IsHost) return false;

            int[] allowed = { 30, 60, 120, 150, 180 };
            if (allowed.Contains(durationSeconds))
            {
                RoundDurationSeconds = durationSeconds;
                return true;
            }
            return false;
        }
    }

    public bool ForceStartGame(string connectionId)
    {
        lock (_lock)
        {
            if (_stage != GameStage.Lobby) return false;
            if (!_lobbyPlayers.TryGetValue(connectionId, out var lp) || !lp.IsHost) return false;
            if (_lobbyPlayers.Count < 2) return false;

            int readyCount = _lobbyPlayers.Values.Count(p => p.IsReady);
            int halfCount = (int)Math.Ceiling(_lobbyPlayers.Count / 2.0);

            if (readyCount >= halfCount)
            {
                StartNewMatchSession();
                return true;
            }
            return false;
        }
    }

    private void CheckAutoStart()
    {
        if (_lobbyPlayers.Count >= 2 && _lobbyPlayers.Values.All(p => p.IsReady))
        {
            StartNewMatchSession();
        }
    }

    private void StartNewMatchSession()
    {
        int normalCount = _lobbyPlayers.Values.Count(p => p.MapVote == "normal");
        int iceCount = _lobbyPlayers.Values.Count(p => p.MapVote == "ice");

        if (normalCount > iceCount)
            SelectedMapType = "normal";
        else if (iceCount > normalCount)
            SelectedMapType = "ice";
        else
            SelectedMapType = Random.Shared.Next(0, 2) == 0 ? "normal" : "ice";

        CurrentRound = 1;
        RemainingSeconds = RoundDurationSeconds;
        RoundWinnerName = string.Empty;
        ChampionName = string.Empty;

        _leaderboard.ResetScores();
        foreach (var lp in _lobbyPlayers.Values)
        {
            _leaderboard.RegisterPlayer(lp.Id, lp.Name);
        }

        _Match.ResetForNewRound(SelectedMapType);
        _stage = GameStage.InGame;
    }

    public void ReturnToLobby()
    {
        lock (_lock)
        {
            _stage = GameStage.Lobby;
            foreach (var lp in _lobbyPlayers.Values)
            {
                lp.IsReady = false;
            }
            CurrentRound = 1;
            RemainingSeconds = RoundDurationSeconds;
            RoundWinnerName = string.Empty;
            ChampionName = string.Empty;
        }
    }

    public string GetFullMap()
    {
        lock (_lock)
        {
            string matchMapJson = _Match.GetFullGameMapJson();
            JsonObject root;
            try
            {
                root = JsonNode.Parse(matchMapJson)?.AsObject() ?? new JsonObject();
            }
            catch
            {
                root = new JsonObject();
            }

            root["Stage"] = _stage.ToString();
            root["CurrentRound"] = CurrentRound;
            root["TotalRounds"] = TotalRounds;
            root["RemainingSeconds"] = (int)Math.Ceiling(RemainingSeconds);
            root["RoundDuration"] = RoundDurationSeconds;
            root["IntermissionCountdown"] = (int)Math.Ceiling(IntermissionCountdown);
            root["SelectedMapType"] = SelectedMapType;
            root["RoundWinner"] = RoundWinnerName;
            root["Champion"] = ChampionName;
            root["SameMapVotes"] = _sameVotes.Count;
            root["SameMapVotesNeeded"] = Math.Max(1, (int)Math.Ceiling(_lobbyPlayers.Count / 2.0));

            // Leaderboard
            var standings = _leaderboard.GetStandings();
            var lbArray = new JsonArray();
            foreach (var s in standings)
            {
                bool isHost = _lobbyPlayers.TryGetValue(s.PlayerId, out var lp) && lp.IsHost;
                lbArray.Add(new JsonObject
                {
                    ["PlayerId"] = s.PlayerId,
                    ["PlayerName"] = s.PlayerName,
                    ["IsHost"] = isHost,
                    ["Kills"] = s.Kills,
                    ["Deaths"] = s.Deaths,
                    ["RoundWins"] = s.RoundWins,
                    ["Score"] = s.Score
                });
            }
            root["Leaderboard"] = lbArray;

            // Lobby info
            int readyCount = _lobbyPlayers.Values.Count(p => p.IsReady);
            int halfCount = (int)Math.Ceiling(_lobbyPlayers.Count / 2.0);
            bool canForce = _lobbyPlayers.Count >= 2 && readyCount >= halfCount;
            bool canAuto = _lobbyPlayers.Count >= 2 && readyCount == _lobbyPlayers.Count;

            var playersArray = new JsonArray();
            foreach (var lp in _lobbyPlayers.Values)
            {
                playersArray.Add(new JsonObject
                {
                    ["Id"] = lp.Id,
                    ["Name"] = lp.Name,
                    ["IsHost"] = lp.IsHost,
                    ["IsReady"] = lp.IsReady,
                    ["MapVote"] = lp.MapVote
                });
            }

            root["Lobby"] = new JsonObject
            {
                ["Players"] = playersArray,
                ["PlayerCount"] = _lobbyPlayers.Count,
                ["TotalRounds"] = TotalRounds,
                ["RoundDurationSeconds"] = RoundDurationSeconds,
                ["CanForceStart"] = canForce,
                ["CanAutoStart"] = canAuto
            };

            return root.ToJsonString();
        }
    }

    public void MovePlayer(string connectionId, int dx, int dy)
    {
        lock (_lock)
        {
            if (_stage != GameStage.InGame) return;

            if (_players.TryGetValue(connectionId, out var player) && player.IsAlive)
            {
                _Match.MovePlayer(dx, dy, connectionId);
            }
        }
    }

    public void Update(float deltaTime)
    {
        lock (_lock)
        {
            if (_stage == GameStage.InGame)
            {
                RemainingSeconds = Math.Max(0, RemainingSeconds - deltaTime);
                _Match.Update(deltaTime);

                var alive = _Match.GetAlivePlayers();
                if (alive.Count <= 1 || RemainingSeconds <= 0)
                {
                    _stage = GameStage.RoundEnd;
                    IntermissionCountdown = 8.0f;
                    _sameVotes.Clear();

                    Player? winner = alive.Count == 1 ? alive[0] : null;
                    RoundWinnerName = winner != null ? winner.Name : "Draw (No Survivors)";

                    _Match.NotifyRoundEnded(winner, CurrentRound, TotalRounds);
                }
            }
            else if (_stage == GameStage.RoundEnd)
            {
                IntermissionCountdown -= deltaTime;
                if (IntermissionCountdown <= 0)
                {
                    if (CurrentRound < TotalRounds)
                    {
                        int needed = Math.Max(1, (int)Math.Ceiling(_lobbyPlayers.Count / 2.0));
                        bool useSameMap = _sameVotes.Count >= needed;
                        _sameVotes.Clear();
                        CurrentRound++;
                        RemainingSeconds = RoundDurationSeconds;
                        _Match.ResetForNewRound(SelectedMapType, useSameMap);
                        _stage = GameStage.InGame;
                    }
                    else
                    {
                        _stage = GameStage.MatchEnd;
                        var standings = _leaderboard.GetStandings();
                        var topPlayer = standings.FirstOrDefault();
                        ChampionName = topPlayer != null ? topPlayer.PlayerName : "Nobody";
                        _Match.NotifyMatchEnded(topPlayer != null ? _players.GetValueOrDefault(topPlayer.PlayerId) : null, standings);
                    }
                }
            }
        }
    }

    public static IExplosionStrategy ResolveStrategy(string? strategyName)
    {
        if (string.IsNullOrWhiteSpace(strategyName))
            return new ClassicCrossExplosionStrategy();

        string norm = strategyName.Trim().ToLowerInvariant();
        if (norm.Contains("diag") || norm == "2") return new DiagonalExplosionStrategy();
        if (norm.Contains("area") || norm.Contains("box") || norm == "3") return new AreaBoxExplosionStrategy();
        if (norm.Contains("pierc") || norm == "4") return new PiercingLineExplosionStrategy();
        if (norm.Contains("flood") || norm == "5") return new FloodFillExplosionStrategy();
        return new ClassicCrossExplosionStrategy();
    }

    public void PlaceBomb(string connectionId, string? strategyName = null)
    {
        lock (_lock)
        {
            if (_stage != GameStage.InGame) return;

            if (_players.TryGetValue(connectionId, out var player) && player.IsAlive)
            {
                IExplosionStrategy strategy = !string.IsNullOrWhiteSpace(strategyName)
                    ? ResolveStrategy(strategyName)
                    : player.ActiveBombStrategy;

                _Match.PlaceBomb(player, strategy);
            }
        }
    }

    public void SetPlayerStrategy(string connectionId, string strategyName)
    {
        lock (_lock)
        {
            if (_players.TryGetValue(connectionId, out var player))
            {
                player.ActiveBombStrategy = ResolveStrategy(strategyName);
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

    public List<Match.ItemDto> GetAllItems()
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
            if (_stage != GameStage.InGame) return;

            if (_players.TryGetValue(connectionId, out var player) && player.IsAlive)
            {
                _Match.UseWeapon(player);
            }
        }
    }

    public void VoteSameMap(string connectionId)
    {
        lock (_lock)
        {
            if(_stage != GameStage.RoundEnd) return;

            if (!_sameVotes.Remove(connectionId))
            {
                _sameVotes.Add(connectionId);
            }
        }
    }

}