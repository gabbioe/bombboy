using BombBoy.Models.Adapter;
using BombBoy.Models;
using BombBoy.Models.Interpreter;
using BombBoy.Models.Mediator;
using BombBoy.Models.Observer;
using BombBoy.Models.Strategies;
using BombBoy.Services.Proxy;
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

public class GameManager : IGameManager
{
    private readonly LobbyService _lobby = new();
    private readonly RoundService _round = new();
    private readonly PlayerService _players = new();
    private readonly CheatService _cheats = new();
    private readonly ConsoleInterpreterClient _consoleInterpreter = new();
    private readonly GameStateService _gameState = new();
    private readonly Match _match = new Match();
    private readonly LeaderboardObserver _leaderboard = new();
    private readonly GameEventLoggerObserver _logger = new();
    private readonly ExternalAudioEngine _audioEngine = new();
    private readonly ISoundEffectService _soundService;
    private readonly AudioObserver _audioObserver;
    private readonly GameChatroomMediator _chatMediator;
    private readonly BountyMasterColleague _bountyColleague;
    private readonly ArenaAnnouncerColleague _announcerColleague;
    private readonly SoundCueColleague _soundCueColleague;

    public event Action<string, float>? OnSoundEffectPlayed;
    public event Action<string, string, string, string>? OnChatMessageDelivered;

    public GameStage Stage => _round.Stage;
    public bool IsHost(string connectionId) => _lobby.IsHost(connectionId);
    public bool CanForceStart(string connectionId) => _lobby.CanForceStart(connectionId);
    public bool IsPlayerAlive(string connectionId) => _players.TryGetAlivePlayer(connectionId, out _);

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

        _match.Attach(_leaderboard);
        _match.Attach(_logger);
        _match.Attach(_audioObserver);

        _chatMediator = new GameChatroomMediator(() => _round.Stage);
        _bountyColleague = new BountyMasterColleague(_chatMediator, _match);
        _announcerColleague = new ArenaAnnouncerColleague(_chatMediator, _match, _round);
        _soundCueColleague = new SoundCueColleague(_chatMediator, _soundService);

        _chatMediator.Register(_bountyColleague);
        _chatMediator.Register(_announcerColleague);
        _chatMediator.Register(_soundCueColleague);

        _match.OnPlayerEliminated += (victim, killer) => _bountyColleague.CheckBountyClaimed(victim, killer);
    }

    public void SetSoundVolume(float volume) => _soundService.SetVolume(volume);
    public float GetSoundVolume() => _soundService.GetVolume();

    public (Player? player, bool isFull) AddPlayer(string connectionId)
    {
        string name = _players.FormatPlayerName(connectionId);
        if (!_lobby.TryAddPlayer(connectionId, name))
            return (null, true);

        _leaderboard.RegisterPlayer(connectionId, name);
        _soundService.PlaySound(SoundEffectType.LobbyJoin);

        var player = _players.RegisterPlayer(connectionId, name, null);
        int[]? startPos = _match.AddPlayer(player);
        if (startPos != null)
        {
            player.X = startPos[0];
            player.Y = startPos[1];
        }

        var chatColleague = new PlayerChatColleague(
            _chatMediator,
            player,
            (recipientId, senderName, msg, channel) => OnChatMessageDelivered?.Invoke(recipientId, senderName, msg, channel));
        _chatMediator.Register(chatColleague);

        return (player, false);
    }

    public void GenerateMap()
    {
        _match.GenerateMap();
        _match.GetMapStringFormat();
    }

    public bool RemovePlayer(string connectionId, out Player? player)
    {
        _lobby.RemovePlayer(connectionId);
        _match.RemovePlayer(connectionId);
        _cheats.RemovePlayer(connectionId);
        _chatMediator.Unregister(connectionId);
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

    public void ToggleReady(string connectionId)
    {
        if (_lobby.TryToggleReady(connectionId) && _lobby.CanAutoStart)
        {
            StartNewMatchSession();
        }
    }

    public void VoteMap(string connectionId, string mapType)
    {
        _lobby.TryVoteMap(connectionId, mapType);
    }

    public bool SetMatchRounds(string connectionId, int rounds) =>
        _round.TrySetMatchRounds(rounds);

    public bool SetMatchDuration(string connectionId, int durationSeconds) =>
        _round.TrySetMatchDuration(durationSeconds);

    public bool ForceStartGame(string connectionId)
    {
        StartNewMatchSession();
        return true;
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
        _bountyColleague.ResetBounty();
        _match.ResetForNewRound(_round.SelectedMapType);
        _match.NotifyRoundStarted(_round.CurrentRound, _round.SelectedMapType);
        _announcerColleague.AnnounceRoundStart(_round.CurrentRound, _round.SelectedMapType);
    }

    public void ReturnToLobby()
    {
        _round.ReturnToLobby();
        _lobby.ResetReadyStatus();
        _cheats.Clear();
    }

    public string GetFullMap() =>
        _gameState.BuildFullMapJson(_match, _round, _lobby, _leaderboard, _cheats);

    public void MovePlayer(string connectionId, int dx, int dy) =>
        _match.MovePlayer(dx, dy, connectionId);

    public void Update(float deltaTime)
    {
        if (_round.Stage == GameStage.InGame)
        {
            _round.TickTimer(deltaTime);
            _match.Update(deltaTime);

            if (_round.TryConcludeRound(_match.GetAlivePlayers(), out var winner))
            {
                _match.NotifyRoundEnded(winner, _round.CurrentRound, _round.TotalRounds);
                _announcerColleague.AnnounceRoundEnd(_round.RoundWinnerName, _round.CurrentRound);
            }
        }
        else if (_round.Stage == GameStage.RoundEnd)
        {
            if (_round.TickIntermission(deltaTime, _lobby.PlayerCount, out bool startNextRound, out bool useSameMap))
            {
                if (startNextRound)
                {
                    _cheats.Clear();
                    _bountyColleague.ResetBounty();
                    _match.ResetForNewRound(_round.SelectedMapType, useSameMap);
                    _match.NotifyRoundStarted(_round.CurrentRound, _round.SelectedMapType);
                    _announcerColleague.AnnounceRoundStart(_round.CurrentRound, _round.SelectedMapType);
                }
                else
                {
                    var standings = _leaderboard.GetStandings();
                    var topPlayer = standings.FirstOrDefault();
                    _round.SetChampion(topPlayer?.PlayerName);
                    _match.NotifyMatchEnded(_players.GetPlayerOrDefault(topPlayer?.PlayerId), standings);
                }
            }
        }
    }

    public static IExplosionStrategy ResolveStrategy(string? strategyName) =>
        PlayerService.ResolveStrategy(strategyName);

    public void PlaceBomb(string connectionId, string? strategyName = null)
    {
        if (_players.TryGetAlivePlayer(connectionId, out var player))
        {
            var strategy = _players.ResolveBombStrategy(player!, strategyName);
            _match.PlaceBomb(player!, strategy);
        }
    }

    public void SetPlayerStrategy(string connectionId, string strategyName) =>
        _players.SetPlayerStrategy(connectionId, strategyName);

    public List<PlayerDto> GetAllPlayers() =>
        _players.GetAllPlayers();

    public record PlayerDto(string Id, string Name, int X, int Y, bool IsAlive);

    public List<Match.ItemDto> GetAllItems() =>
        _match.GetAllItems();

    public void UseWeapon(string connectionId)
    {
        if (_players.TryGetAlivePlayer(connectionId, out var player))
        {
            _match.UseWeapon(player!);
        }
    }

    public void VoteSameMap(string connectionId) =>
        _round.ToggleSameMapVote(connectionId);

    public void ActivateCheat(string connectionId, string cheatCode)
    {
        if (_players.TryGetAlivePlayer(connectionId, out var player))
        {
            _cheats.ActivateCheat(player!, cheatCode);
        }
    }

    public void UndoCheat(string connectionId, string cheatCode) =>
        _cheats.UndoCheat(connectionId, cheatCode);

    public string ExecuteConsoleCommand(string connectionId, string commandLine)
    {
        if (!_players.TryGetPlayer(connectionId, out var caller) || caller == null)
            return "[Error] Caller player not found.";

        var context = new ConsoleContext(commandLine, caller, _match, _round, _cheats);
        return _consoleInterpreter.Execute(context);
    }

    public void SendChatMessage(string connectionId, string message)
    {
        var senderColleague = _chatMediator.GetPlayerColleague(connectionId);
        senderColleague?.Send(message);
    }
}
