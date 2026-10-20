namespace BombBoy.Models.Observer;

public interface IGameEventSubject
{
    void Attach(IGameEventObserver observer);
    void Detach(IGameEventObserver observer);
    void NotifyPlayerDied(Player victim, Player? killer);
    void NotifyRoundEnded(Player? winner, int roundNumber, int totalRounds);
    void NotifyMatchEnded(Player? champion, List<PlayerStats> finalStandings);
}
