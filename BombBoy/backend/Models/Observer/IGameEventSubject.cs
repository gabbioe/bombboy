using System.Collections.Generic;

namespace BombBoy.Models.Observer;

public interface IGameEventSubject
{
    void Attach(IGameEventObserver observer);
    void Detach(IGameEventObserver observer);
    void NotifyPlayerDied(Player victim, Player? killer);
    void NotifyRoundEnded(Player? winner, int roundNumber, int totalRounds);
    void NotifyMatchEnded(Player? champion, List<PlayerStats> finalStandings);

    void NotifyBombPlanted(Player player);
    void NotifyBombExploded(int x, int y);
    void NotifyPlayerDamaged(Player victim, Player? attacker = null);
    void NotifyPlayerDrowned(Player victim);
    void NotifyItemSpawned(int x, int y);
    void NotifyItemPickedUp(Player player, string itemType);
    void NotifyTileDestroyed(int x, int y);
    void NotifyWeaponUsed(Player player, string weaponName);
    void NotifyRoundStarted(int roundNumber, string mapType);
    void NotifyPlayerMoved(Player player, string mapType);
    void NotifyPlayerJoined(string playerName);
    void NotifyPlayerLeft(string playerName);
}
