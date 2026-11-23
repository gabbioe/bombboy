using BombBoy.Models;
using System;
using System.Collections.Generic;

namespace BombBoy.Services.Proxy;

/// <summary>
/// Protection & Synchronization Proxy for GameManager.
/// Enforces thread-safety locks, game stage preconditions, alive-player checks,
/// and host authorization rules before delegating to the underlying GameManager.
/// </summary>
public sealed class GameManagerProtectionProxy : IGameManager
{
    private readonly object _lock = new();
    private readonly GameManager _realManager;
    private string? _cachedFullMapJson;

    public GameManagerProtectionProxy(GameManager realManager)
    {
        _realManager = realManager ?? throw new ArgumentNullException(nameof(realManager));
    }

    private void InvalidateStateCache() => _cachedFullMapJson = null;

    public event Action<string, float>? OnSoundEffectPlayed
    {
        add => _realManager.OnSoundEffectPlayed += value;
        remove => _realManager.OnSoundEffectPlayed -= value;
    }

    public int TotalRounds
    {
        get { lock (_lock) return _realManager.TotalRounds; }
        set { lock (_lock) { _realManager.TotalRounds = value; InvalidateStateCache(); } }
    }

    public int RoundDurationSeconds
    {
        get { lock (_lock) return _realManager.RoundDurationSeconds; }
        set { lock (_lock) { _realManager.RoundDurationSeconds = value; InvalidateStateCache(); } }
    }

    public int CurrentRound { get { lock (_lock) return _realManager.CurrentRound; } }
    public float RemainingSeconds { get { lock (_lock) return _realManager.RemainingSeconds; } }
    public float IntermissionCountdown { get { lock (_lock) return _realManager.IntermissionCountdown; } }
    public string SelectedMapType { get { lock (_lock) return _realManager.SelectedMapType; } }
    public string RoundWinnerName { get { lock (_lock) return _realManager.RoundWinnerName; } }
    public string ChampionName { get { lock (_lock) return _realManager.ChampionName; } }

    public void SetSoundVolume(float volume) => _realManager.SetSoundVolume(volume);
    public float GetSoundVolume() => _realManager.GetSoundVolume();

    public (Player? player, bool isFull) AddPlayer(string connectionId)
    {
        lock (_lock)
        {
            InvalidateStateCache();
            return _realManager.AddPlayer(connectionId);
        }
    }

    public void GenerateMap()
    {
        lock (_lock)
        {
            InvalidateStateCache();
            _realManager.GenerateMap();
        }
    }

    public bool RemovePlayer(string connectionId, out Player? player)
    {
        lock (_lock)
        {
            InvalidateStateCache();
            return _realManager.RemovePlayer(connectionId, out player);
        }
    }

    public void ToggleReady(string connectionId)
    {
        lock (_lock)
        {
            if (_realManager.Stage != GameStage.Lobby)
                return;

            InvalidateStateCache();
            _realManager.ToggleReady(connectionId);
        }
    }

    public void VoteMap(string connectionId, string mapType)
    {
        lock (_lock)
        {
            if (_realManager.Stage != GameStage.Lobby)
                return;

            InvalidateStateCache();
            _realManager.VoteMap(connectionId, mapType);
        }
    }

    public bool SetMatchRounds(string connectionId, int rounds)
    {
        lock (_lock)
        {
            if (_realManager.Stage != GameStage.Lobby || !_realManager.IsHost(connectionId))
                return false;

            InvalidateStateCache();
            return _realManager.SetMatchRounds(connectionId, rounds);
        }
    }

    public bool SetMatchDuration(string connectionId, int durationSeconds)
    {
        lock (_lock)
        {
            if (_realManager.Stage != GameStage.Lobby || !_realManager.IsHost(connectionId))
                return false;

            InvalidateStateCache();
            return _realManager.SetMatchDuration(connectionId, durationSeconds);
        }
    }

    public bool ForceStartGame(string connectionId)
    {
        lock (_lock)
        {
            if (_realManager.Stage != GameStage.Lobby || !_realManager.CanForceStart(connectionId))
                return false;

            InvalidateStateCache();
            return _realManager.ForceStartGame(connectionId);
        }
    }

    public void ReturnToLobby()
    {
        lock (_lock)
        {
            InvalidateStateCache();
            _realManager.ReturnToLobby();
        }
    }

    public string GetFullMap()
    {
        lock (_lock)
        {
            return _cachedFullMapJson ??= _realManager.GetFullMap();
        }
    }

    public void MovePlayer(string connectionId, int dx, int dy)
    {
        lock (_lock)
        {
            if (_realManager.Stage != GameStage.InGame || !_realManager.IsPlayerAlive(connectionId))
                return;

            InvalidateStateCache();
            _realManager.MovePlayer(connectionId, dx, dy);
        }
    }

    public void Update(float deltaTime)
    {
        lock (_lock)
        {
            InvalidateStateCache();
            _realManager.Update(deltaTime);
        }
    }

    public void PlaceBomb(string connectionId, string? strategyName = null)
    {
        lock (_lock)
        {
            if (_realManager.Stage != GameStage.InGame || !_realManager.IsPlayerAlive(connectionId))
                return;

            InvalidateStateCache();
            _realManager.PlaceBomb(connectionId, strategyName);
        }
    }

    public void SetPlayerStrategy(string connectionId, string strategyName)
    {
        lock (_lock)
        {
            InvalidateStateCache();
            _realManager.SetPlayerStrategy(connectionId, strategyName);
        }
    }

    public List<GameManager.PlayerDto> GetAllPlayers()
    {
        lock (_lock)
        {
            return _realManager.GetAllPlayers();
        }
    }

    public List<Match.ItemDto> GetAllItems()
    {
        lock (_lock)
        {
            return _realManager.GetAllItems();
        }
    }

    public void UseWeapon(string connectionId)
    {
        lock (_lock)
        {
            if (_realManager.Stage != GameStage.InGame || !_realManager.IsPlayerAlive(connectionId))
                return;

            InvalidateStateCache();
            _realManager.UseWeapon(connectionId);
        }
    }

    public void VoteSameMap(string connectionId)
    {
        lock (_lock)
        {
            if (_realManager.Stage != GameStage.RoundEnd)
                return;

            InvalidateStateCache();
            _realManager.VoteSameMap(connectionId);
        }
    }

    public void ActivateCheat(string connectionId, string cheatCode)
    {
        lock (_lock)
        {
            if (_realManager.Stage != GameStage.InGame || !_realManager.IsPlayerAlive(connectionId))
                return;

            InvalidateStateCache();
            _realManager.ActivateCheat(connectionId, cheatCode);
        }
    }

    public void UndoCheat(string connectionId, string cheatCode)
    {
        lock (_lock)
        {
            if (_realManager.Stage != GameStage.InGame)
                return;

            InvalidateStateCache();
            _realManager.UndoCheat(connectionId, cheatCode);
        }
    }
}
