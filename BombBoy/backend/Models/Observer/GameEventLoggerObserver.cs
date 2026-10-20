using System;
using System.Collections.Generic;

namespace BombBoy.Models.Observer;

public class GameEventLoggerObserver : IGameEventObserver
{
    public void OnPlayerDied(Player victim, Player? killer)
    {
        if (killer != null && killer.Id != victim.Id)
        {
            Console.WriteLine($"[GameEventLogger] 💀 {victim.Name} was eliminated by {killer.Name}!");
        }
        else
        {
            Console.WriteLine($"[GameEventLogger] 💀 {victim.Name} died!");
        }
    }

    public void OnRoundEnded(Player? winner, int roundNumber, int totalRounds)
    {
        string winnerName = winner != null ? winner.Name : "Draw (No survivors)";
        Console.WriteLine($"[GameEventLogger] 🏁 Round {roundNumber}/{totalRounds} ended. Winner: {winnerName}");
    }

    public void OnMatchEnded(Player? champion, List<PlayerStats> finalStandings)
    {
        string champName = champion != null ? champion.Name : "Nobody";
        Console.WriteLine($"[GameEventLogger] 🏆 MATCH COMPLETED! Overall Champion: {champName}");
    }
}
