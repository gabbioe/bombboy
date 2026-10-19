using System;
using System.Collections.Generic;
using System.Linq;

namespace BombBoy.Models.Observer;

public class LeaderboardObserver : IGameEventObserver
{
    private readonly Dictionary<string, PlayerStats> _stats = new();

    public void RegisterPlayer(string id, string name)
    {
        if (!_stats.ContainsKey(id))
        {
            _stats[id] = new PlayerStats
            {
                PlayerId = id,
                PlayerName = name,
                Kills = 0,
                Deaths = 0,
                RoundWins = 0,
                Score = 0
            };
        }
        else
        {
            _stats[id].PlayerName = name;
        }
    }

    public void RemovePlayer(string id)
    {
        _stats.Remove(id);
    }

    public void ResetScores()
    {
        foreach (var s in _stats.Values)
        {
            s.Kills = 0;
            s.Deaths = 0;
            s.RoundWins = 0;
            s.Score = 0;
        }
    }

    public void Clear()
    {
        _stats.Clear();
    }

    public void OnPlayerDied(Player victim, Player? killer)
    {
        RegisterPlayer(victim.Id, victim.Name);
        _stats[victim.Id].Deaths++;
        victim.DeathCount++;

        if (killer != null && killer.Id != victim.Id)
        {
            RegisterPlayer(killer.Id, killer.Name);
            _stats[killer.Id].Kills++;
            _stats[killer.Id].Score += 100;
            killer.KillCount++;
        }
    }

    public void OnRoundEnded(Player? winner, int roundNumber, int totalRounds)
    {
        if (winner != null)
        {
            RegisterPlayer(winner.Id, winner.Name);
            _stats[winner.Id].RoundWins++;
            _stats[winner.Id].Score += 250;
        }
    }

    public void OnMatchEnded(Player? champion, List<PlayerStats> finalStandings)
    {
        // Standings finalized for this match session
    }

    public List<PlayerStats> GetStandings()
    {
        return _stats.Values
            .OrderByDescending(s => s.Score)
            .ThenByDescending(s => s.RoundWins)
            .ThenByDescending(s => s.Kills)
            .ThenBy(s => s.Deaths)
            .ToList();
    }
}
