using BombBoy.Models;
using System;
using System.Collections.Generic;
using System.Linq;

namespace BombBoy.Services;

public sealed class RoundService
{
    private static readonly int[] AllowedDurations = { 30, 60, 120, 150, 180 };
    private readonly HashSet<string> _sameVotes = new();

    public GameStage Stage { get; private set; } = GameStage.Lobby;
    public int TotalRounds { get; set; } = 5;
    public int RoundDurationSeconds { get; set; } = 180;
    public int CurrentRound { get; private set; } = 1;
    public float RemainingSeconds { get; private set; } = 180f;
    public float IntermissionCountdown { get; private set; } = 0f;
    public string SelectedMapType { get; private set; } = "normal";
    public string RoundWinnerName { get; private set; } = string.Empty;
    public string ChampionName { get; private set; } = string.Empty;

    public int SameMapVotesCount => _sameVotes.Count;

    public int GetSameMapVotesNeeded(int playerCount) =>
        Math.Max(1, (int)Math.Ceiling(playerCount / 2.0));

    public bool TrySetMatchRounds(int rounds)
    {
        if (Stage != GameStage.Lobby)
            return false;

        TotalRounds = Math.Clamp(rounds, 1, 10);
        return true;
    }

    public bool TrySetMatchDuration(int durationSeconds)
    {
        if (Stage != GameStage.Lobby || !AllowedDurations.Contains(durationSeconds))
            return false;

        RoundDurationSeconds = durationSeconds;
        return true;
    }

    public void StartNewMatch(string selectedMapType)
    {
        SelectedMapType = selectedMapType;
        CurrentRound = 1;
        RemainingSeconds = RoundDurationSeconds;
        RoundWinnerName = string.Empty;
        ChampionName = string.Empty;
        Stage = GameStage.InGame;
    }

    public void ReturnToLobby()
    {
        Stage = GameStage.Lobby;
        CurrentRound = 1;
        RemainingSeconds = RoundDurationSeconds;
        RoundWinnerName = string.Empty;
        ChampionName = string.Empty;
    }

    public void ResetToEmptyLobby()
    {
        Stage = GameStage.Lobby;
        CurrentRound = 1;
    }

    public void ToggleSameMapVote(string connectionId)
    {
        if (Stage != GameStage.RoundEnd)
            return;

        if (!_sameVotes.Remove(connectionId))
        {
            _sameVotes.Add(connectionId);
        }
    }

    public void TickTimer(float deltaTime)
    {
        RemainingSeconds = Math.Max(0, RemainingSeconds - deltaTime);
    }

    public bool TryConcludeRound(IReadOnlyList<Player> alivePlayers, out Player? winner)
    {
        winner = null;
        if (alivePlayers.Count > 1 && RemainingSeconds > 0)
            return false;

        Stage = GameStage.RoundEnd;
        IntermissionCountdown = 8.0f;
        _sameVotes.Clear();

        winner = alivePlayers.Count == 1 ? alivePlayers[0] : null;
        RoundWinnerName = winner != null ? winner.Name : "Draw (No Survivors)";
        return true;
    }

    public bool TickIntermission(float deltaTime, int playerCount, out bool startNextRound, out bool useSameMap)
    {
        startNextRound = false;
        useSameMap = false;

        IntermissionCountdown -= deltaTime;
        if (IntermissionCountdown > 0)
            return false;

        if (CurrentRound < TotalRounds)
        {
            int needed = GetSameMapVotesNeeded(playerCount);
            useSameMap = _sameVotes.Count >= needed;
            _sameVotes.Clear();
            CurrentRound++;
            RemainingSeconds = RoundDurationSeconds;
            Stage = GameStage.InGame;
            startNextRound = true;
        }
        else
        {
            Stage = GameStage.MatchEnd;
        }

        return true;
    }

    public void SetChampion(string? championName)
    {
        ChampionName = championName ?? "Nobody";
    }
}
