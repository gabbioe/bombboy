using BombBoy.Models.Adapter;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;

namespace BombBoy.Models.Observer;

/// <summary>
/// Client & Observer: stebi žaidimo įvykius ir, panaudodamas ISoundEffectService sąsają (Target),
/// inicijuoja atitinkamų garso efektų grojimą. Turi apsaugas nuo garso dubliavimo / kilpų (cooldown guard).
/// </summary>
public class AudioObserver : IGameEventObserver
{
    private readonly ISoundEffectService _soundService;
    private readonly ConcurrentDictionary<SoundEffectType, DateTime> _lastPlayed = new();

    public AudioObserver(ISoundEffectService soundService)
    {
        _soundService = soundService;
    }

    private void PlayGuarded(SoundEffectType type, TimeSpan minInterval)
    {
        var now = DateTime.UtcNow;
        if (_lastPlayed.TryGetValue(type, out var last) && (now - last) < minInterval)
        {
            return; // užkerta kelią pasikartojimui per trumpą laiko tarpą
        }
        _lastPlayed[type] = now;
        _soundService.PlaySound(type);
    }

    public void OnPlayerDied(Player victim, Player? killer)
    {
        PlayGuarded(SoundEffectType.PlayerDeath, TimeSpan.FromMilliseconds(400));
    }

    public void OnPlayerDamaged(Player victim, Player? attacker)
    {
        PlayGuarded(SoundEffectType.PlayerHurt, TimeSpan.FromMilliseconds(250));
    }

    public void OnPlayerDrowned(Player victim)
    {
        PlayGuarded(SoundEffectType.WaterDrown, TimeSpan.FromSeconds(2));
    }

    public void OnBombPlanted(Player player)
    {
        PlayGuarded(SoundEffectType.BombPlace, TimeSpan.FromMilliseconds(100));
    }

    public void OnBombExploded(int x, int y)
    {
        PlayGuarded(SoundEffectType.BombExplode, TimeSpan.FromMilliseconds(80));
    }

    public void OnRoundStarted(int roundNumber, string mapType)
    {
        PlayGuarded(SoundEffectType.RoundStart, TimeSpan.FromSeconds(3));
    }

    public void OnRoundEnded(Player? winner, int roundNumber, int totalRounds)
    {
        PlayGuarded(SoundEffectType.RoundEnd, TimeSpan.FromSeconds(6));
    }

    public void OnMatchEnded(Player? champion, List<PlayerStats> finalStandings)
    {
        PlayGuarded(SoundEffectType.RoundEnd, TimeSpan.FromSeconds(6));
    }

    public void OnItemSpawned(int x, int y)
    {
        PlayGuarded(SoundEffectType.ItemAppear, TimeSpan.FromMilliseconds(150));
    }

    public void OnItemPickedUp(Player player, string itemType)
    {
        PlayGuarded(SoundEffectType.ItemPickup, TimeSpan.FromMilliseconds(150));
    }

    public void OnTileDestroyed(int x, int y)
    {
        PlayGuarded(SoundEffectType.TileDestroy, TimeSpan.FromMilliseconds(100));
    }

    public void OnWeaponUsed(Player player, string weaponName)
    {
        SoundEffectType effect = weaponName.ToLowerInvariant() switch
        {
            "lasergun" => SoundEffectType.LaserGun,
            "frozenlasergun" => SoundEffectType.FrozenLaserGun,
            "knife" => SoundEffectType.Knife,
            "popsicle" => SoundEffectType.Popsicle,
            "throwableweapon" or "throwable" => SoundEffectType.Throwable,
            "snowballweapon" or "snowball" => SoundEffectType.SnowballWeapon,
            _ => SoundEffectType.LaserGun
        };
        PlayGuarded(effect, TimeSpan.FromMilliseconds(200));
    }

    public void OnPlayerMoved(Player player, string mapType)
    {
        if (mapType != null && mapType.ToLowerInvariant() == "ice")
            PlayGuarded(SoundEffectType.StepIce, TimeSpan.FromMilliseconds(120));
        else
            PlayGuarded(SoundEffectType.StepGround, TimeSpan.FromMilliseconds(120));
    }

    public void OnPlayerJoined(string playerName)
    {
        PlayGuarded(SoundEffectType.LobbyJoin, TimeSpan.FromMilliseconds(300));
    }

    public void OnPlayerLeft(string playerName)
    {
        PlayGuarded(SoundEffectType.PlayerLeave, TimeSpan.FromMilliseconds(300));
    }

    public void OnButtonClicked()
    {
        PlayGuarded(SoundEffectType.ButtonClick, TimeSpan.FromMilliseconds(50));
    }
}
