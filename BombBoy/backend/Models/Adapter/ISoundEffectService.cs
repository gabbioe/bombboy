namespace BombBoy.Models.Adapter;

public enum SoundEffectType
{
    BombPlace,
    BombExplode,
    PlayerHurt,
    PlayerDeath,
    StepIce,
    StepGround,
    WaterDrown,
    ItemAppear,
    ItemPickup,
    TileDestroy,
    LaserGun,
    Knife,
    Throwable,
    FrozenLaserGun,
    Popsicle,
    SnowballWeapon,
    RoundStart,
    RoundEnd,
    LobbyJoin,
    PlayerLeave,
    ButtonClick
}

/// <summary>
/// adapterio Target - Vieninga aukšto lygio garso efektų valdymo sąsaja žaidimo branduoliui.
/// </summary>
public interface ISoundEffectService
{
    void PlaySound(SoundEffectType soundEffect, float volume = 1.0f);
    void SetVolume(float volume);
    float GetVolume();
}
