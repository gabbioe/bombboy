using System.Collections.Generic;

namespace BombBoy.Models.Observer;

public interface IGameEventObserver
{
    void OnPlayerDied(Player victim, Player? killer);
    void OnRoundEnded(Player? winner, int roundNumber, int totalRounds);
    void OnMatchEnded(Player? champion, List<PlayerStats> finalStandings);
    void OnBombPlanted(Player player) { }
    void OnBombExploded(int x, int y) { }
    void OnPlayerDamaged(Player victim, Player? attacker) { }
    void OnPlayerDrowned(Player victim) { }
    void OnItemSpawned(int x, int y) { }
    void OnItemPickedUp(Player player, string itemType) { }
    void OnTileDestroyed(int x, int y) { }
    void OnWeaponUsed(Player player, string weaponName) { }
    void OnRoundStarted(int roundNumber, string mapType) { }
    void OnPlayerMoved(Player player, string mapType) { }
    void OnPlayerJoined(string playerName) { }
    void OnPlayerLeft(string playerName) { }
    void OnButtonClicked() { }
}
