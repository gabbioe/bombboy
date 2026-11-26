using BombBoy.Models;
using System;
using System.Collections.Generic;

namespace BombBoy.Services.Proxy;

public interface IGameManager
{
    event Action<string, float>? OnSoundEffectPlayed;

    int TotalRounds { get; set; }
    int RoundDurationSeconds { get; set; }
    int CurrentRound { get; }
    float RemainingSeconds { get; }
    float IntermissionCountdown { get; }
    string SelectedMapType { get; }
    string RoundWinnerName { get; }
    string ChampionName { get; }

    void SetSoundVolume(float volume);
    float GetSoundVolume();
    (Player? player, bool isFull) AddPlayer(string connectionId);
    void GenerateMap();
    bool RemovePlayer(string connectionId, out Player? player);
    void ToggleReady(string connectionId);
    void VoteMap(string connectionId, string mapType);
    bool SetMatchRounds(string connectionId, int rounds);
    bool SetMatchDuration(string connectionId, int durationSeconds);
    bool ForceStartGame(string connectionId);
    void ReturnToLobby();
    string GetFullMap();
    void MovePlayer(string connectionId, int dx, int dy);
    void Update(float deltaTime);
    void PlaceBomb(string connectionId, string? strategyName = null);
    void SetPlayerStrategy(string connectionId, string strategyName);
    List<GameManager.PlayerDto> GetAllPlayers();
    List<Match.ItemDto> GetAllItems();
    void UseWeapon(string connectionId);
    void VoteSameMap(string connectionId);
    void ActivateCheat(string connectionId, string cheatCode);
    void UndoCheat(string connectionId, string cheatCode);
    string ExecuteConsoleCommand(string connectionId, string commandLine);
}
