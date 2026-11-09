using BombBoy.Models.Adapter;
using BombBoy.Models;
using BombBoy.Models.Observer;
using BombBoy.Models.Strategies;
using System;
using System.Collections.Generic;
using System.Linq;

namespace BombBoy.Services;

public enum GameStage
{
    Lobby,
    InGame,
    RoundEnd,
    MatchEnd
}

public class GameManager
{
    private readonly object _lock = new();

    private readonly LobbyService _lobby = new();
    private readonly RoundService _round = new();
    private readonly PlayerService _players = new();
    private readonly CheatService _cheats = new();
    private readonly GameStateService _gameState = new();
    private readonly Match _Match = new Match();
    private readonly LeaderboardObserver _leaderboard = new();
    private readonly GameEventLoggerObserver _logger = new();
    private readonly ExternalAudioEngine _audioEngine = new();
    private readonly ISoundEffectService _soundService;
    private readonly AudioObserver _audioObserver;

    public event Action<string, float>? OnSoundEffectPlayed;

    public int TotalRounds
    {
        get => _round.TotalRounds;
        set => _round.TotalRounds = value;
    }

    public int RoundDurationSeconds
    {
        get => _round.RoundDurationSeconds;
        set => _round.RoundDurationSeconds = value;
    }

    public int CurrentRound => _round.CurrentRound;
    public float RemainingSeconds => _round.RemainingSeconds;
    public float IntermissionCountdown => _round.IntermissionCountdown;
    public string SelectedMapType => _round.SelectedMapType;
    public string RoundWinnerName => _round.RoundWinnerName;
    public string ChampionName => _round.ChampionName;

    public GameManager()
    {
        _soundService = new SoundEffectAdapter(_audioEngine, (soundFile, vol) => OnSoundEffectPlayed?.Invoke(soundFile, vol));
        _audioObserver = new AudioObserver(_soundService);

        _Match.Attach(_leaderboard);
        _Match.Attach(_logger);
        _Match.Attach(_audioObserver);
    }

    public void SetSoundVolume(float volume) => _soundService.SetVolume(volume);
    public float GetSoundVolume() => _soundService.GetVolume();

    public (Player? player, bool isFull) AddPlayer(string connectionId)
    {
        lock (_lock)
        {
            string name = _players.FormatPlayerName(connectionId);
            if (!_lobby.TryAddPlayer(connectionId, name))
                return (null, true);

            _leaderboard.RegisterPlayer(connectionId, name);
            _soundService.PlaySound(SoundEffectType.LobbyJoin);

            var player = _players.RegisterPlayer(connectionId, name, null);
            int[]? startPos = _Match.AddPlayer(player);
            if (startPos != null)
            {
                player.X = startPos[0];
                player.Y = startPos[1];
            }

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
            _lobby.RemovePlayer(connectionId);
            _Match.RemovePlayer(connectionId);
            _cheats.RemovePlayer(connectionId);
            bool removed = _players.TryRemovePlayer(connectionId, out player);

            if (_round.Stage == GameStage.Lobby)
            {
                _leaderboard.RemovePlayer(connectionId);
            }

            _soundService.PlaySound(SoundEffectType.PlayerLeave);

            if (_lobby.PlayerCount == 0)
            {
                _round.ResetToEmptyLobby();
                _leaderboard.Clear();
            }

            return removed;
        }
    }

    public void ToggleReady(string connectionId)
    {
        lock (_lock)
        {
            if (_round.Stage != GameStage.Lobby) return;

            if (_lobby.TryToggleReady(connectionId) && _lobby.CanAutoStart)
            {
                StartNewMatchSession();
            }
        }
    }

    public void VoteMap(string connectionId, string mapType)
    {
        lock (_lock)
        {
            if (_round.Stage != GameStage.Lobby) return;

            _lobby.TryVoteMap(connectionId, mapType);
        }
    }

    public bool SetMatchRounds(string connectionId, int rounds)
    {
        lock (_lock)
        {
            return _lobby.IsHost(connectionId) && _round.TrySetMatchRounds(rounds);
        }
    }

    public bool SetMatchDuration(string connectionId, int durationSeconds)
    {
        lock (_lock)
        {
            return _lobby.IsHost(connectionId) && _round.TrySetMatchDuration(durationSeconds);
        }
    }

    public bool ForceStartGame(string connectionId)
    {
        lock (_lock)
        {
            if (_round.Stage != GameStage.Lobby || !_lobby.CanForceStart(connectionId))
                return false;

            StartNewMatchSession();
            return true;
        }
    }

    private void StartNewMatchSession()
    {
        string selectedMapType = _lobby.ChooseMapType();
        _round.StartNewMatch(selectedMapType);

        _leaderboard.Clear();
        foreach (var lp in _lobby.GetPlayers())
        {
            _leaderboard.RegisterPlayer(lp.Id, lp.Name);
        }

        _cheats.Clear();
        _Match.ResetForNewRound(_round.SelectedMapType);
        _Match.NotifyRoundStarted(_round.CurrentRound, _round.SelectedMapType);
    }

    public void ReturnToLobby()
    {
        lock (_lock)
        {
            _round.ReturnToLobby();
            _lobby.ResetReadyStatus();
            _cheats.Clear();
        }
    }

    public string GetFullMap()
    {
        lock (_lock)
        {
            return _gameState.BuildFullMapJson(_Match, _round, _lobby, _leaderboard, _cheats);
        }
    }

    public void MovePlayer(string connectionId, int dx, int dy)
    {
        lock (_lock)
        {
            if (_round.Stage == GameStage.InGame && _players.TryGetAlivePlayer(connectionId, out _))
            {
                _Match.MovePlayer(dx, dy, connectionId);
            }
        }
    }

    public void Update(float deltaTime)
    {
        lock (_lock)
        {
            if (_round.Stage == GameStage.InGame)
            {
                _round.TickTimer(deltaTime);
                _Match.Update(deltaTime);

                if (_round.TryConcludeRound(_Match.GetAlivePlayers(), out var winner))
                {
                    _Match.NotifyRoundEnded(winner, _round.CurrentRound, _round.TotalRounds);
                }
            }
            else if (_round.Stage == GameStage.RoundEnd)
            {
                if (_round.TickIntermission(deltaTime, _lobby.PlayerCount, out bool startNextRound, out bool useSameMap))
                {
                    if (startNextRound)
                    {
                        _cheats.Clear();
                        _Match.ResetForNewRound(_round.SelectedMapType, useSameMap);
                        _Match.NotifyRoundStarted(_round.CurrentRound, _round.SelectedMapType);
                    }
                    else
                    {
                        var standings = _leaderboard.GetStandings();
                        var topPlayer = standings.FirstOrDefault();
                        _round.SetChampion(topPlayer?.PlayerName);
                        _Match.NotifyMatchEnded(_players.GetPlayerOrDefault(topPlayer?.PlayerId), standings);
                    }
                }
            }
        }
    }

    public static IExplosionStrategy ResolveStrategy(string? strategyName) =>
        PlayerService.ResolveStrategy(strategyName);

    public void PlaceBomb(string connectionId, string? strategyName = null)
    {
        lock (_lock)
        {
            if (_round.Stage == GameStage.InGame && _players.TryGetAlivePlayer(connectionId, out var player))
            {
                var strategy = _players.ResolveBombStrategy(player!, strategyName);
                _Match.PlaceBomb(player!, strategy);
            }
        }
    }

    public void SetPlayerStrategy(string connectionId, string strategyName)
    {
        lock (_lock)
        {
            _players.SetPlayerStrategy(connectionId, strategyName);
        }
    }

    public List<PlayerDto> GetAllPlayers()
    {
        lock (_lock)
        {
            return _players.GetAllPlayers();
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
            if (_round.Stage == GameStage.InGame && _players.TryGetAlivePlayer(connectionId, out var player))
            {
                _Match.UseWeapon(player!);
            }
        }
    }

    public void VoteSameMap(string connectionId)
    {
        lock (_lock)
        {
            _round.ToggleSameMapVote(connectionId);
        }
    }

    public void ActivateCheat(string connectionId, string cheatCode)
    {
        lock (_lock)
        {
            if (_round.Stage == GameStage.InGame && _players.TryGetAlivePlayer(connectionId, out var player))
            {
                _cheats.ActivateCheat(player!, cheatCode);
            }
        }
    }

    public void UndoCheat(string connectionId, string cheatCode)
    {
        lock (_lock)
        {
            if (_round.Stage == GameStage.InGame)
            {
                _cheats.UndoCheat(connectionId, cheatCode);
            }
        }
    }
}
