namespace BombBoy.Models.Observer;

public interface IGameEventObserver
{
    void OnPlayerDied(Player victim, Player? killer);
    void OnRoundEnded(Player? winner, int roundNumber, int totalRounds);
    void OnMatchEnded(Player? champion, List<PlayerStats> finalStandings);
}
