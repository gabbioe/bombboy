using System;
using System.Collections.Generic;

namespace BombBoy.Models.Adapter;

/// <summary>
/// Adapter: Adaptuoja nesuderinamą 'ExternalAudioEngine' prie 'ISoundEffectService' sąsajos.
/// konvertuoja žaidimo 'SoundEffectType' enum į konkrečius garso failų pavadinimus bei kanalus,
/// o float garsumą (0.0..1.0) į baitinį parametrą (0..255).
/// </summary>
public class SoundEffectAdapter : ISoundEffectService
{
    private readonly ExternalAudioEngine _adaptee;
    private readonly Action<string, float>? _soundBroadcaster;
    private float _volume = 0.7f; // pradinis 70%

    // atitikmenų žodynas: domeno įvykis -> fizinis failas ir kanalas
    private static readonly Dictionary<SoundEffectType, (string fileName, int channel)> SoundMappings = new()
    {
        [SoundEffectType.BombPlace] = ("bombplace.mp3", 1),
        [SoundEffectType.BombExplode] = ("bombexplode.mp3", 2),
        [SoundEffectType.PlayerHurt] = ("hurt.mp3", 3),
        [SoundEffectType.PlayerDeath] = ("hurt.mp3", 4),
        [SoundEffectType.StepIce] = ("walkice.mp3", 5),
        [SoundEffectType.StepGround] = ("walknormal.mp3", 5),
        [SoundEffectType.WaterDrown] = ("water.mp3", 6),
        [SoundEffectType.ItemAppear] = ("itemappear.mp3", 7),
        [SoundEffectType.ItemPickup] = ("itempickup.mp3", 8),
        [SoundEffectType.TileDestroy] = ("destructabletilebreak.mp3", 9),
        [SoundEffectType.LaserGun] = ("lasergun.mp3", 10),
        [SoundEffectType.Knife] = ("knife.mp3", 10),
        [SoundEffectType.Throwable] = ("throwable.mp3", 10),
        [SoundEffectType.FrozenLaserGun] = ("frozenlasergun.mp3", 10),
        [SoundEffectType.Popsicle] = ("popsicle.mp3", 10),
        [SoundEffectType.SnowballWeapon] = ("snowballweapon.mp3", 10),
        [SoundEffectType.RoundStart] = ("roundstart.mp3", 11),
        [SoundEffectType.RoundEnd] = ("roundend.mp3", 12),
        [SoundEffectType.LobbyJoin] = ("playerjoin.mp3", 13),
        [SoundEffectType.PlayerLeave] = ("playerleave.mp3", 13),
        [SoundEffectType.ButtonClick] = ("click.mp3", 14)
    };

    public SoundEffectAdapter(ExternalAudioEngine adaptee, Action<string, float>? soundBroadcaster = null)
    {
        _adaptee = adaptee ?? throw new ArgumentNullException(nameof(adaptee));
        _soundBroadcaster = soundBroadcaster;
        SetVolume(_volume);
    }

    public void PlaySound(SoundEffectType soundEffect, float volume = 1.0f)
    {
        if (SoundMappings.TryGetValue(soundEffect, out var mapping))
        {
            float clampedRelative = Math.Clamp(volume, 0.0f, 1.0f);
            byte gainLevel = (byte)Math.Clamp((int)(clampedRelative * 255), 0, 255);

            // deleguojama Adaptee metodui su adaptuotais parametrais
            _adaptee.ExecuteSoundCommand(mapping.fileName, mapping.channel, gainLevel, false);

            // pranešama klientų SignalR transliavimo kanalui
            _soundBroadcaster?.Invoke(mapping.fileName, _volume * clampedRelative);
        }
    }

    public void SetVolume(float volume)
    {
        _volume = Math.Clamp(volume, 0.0f, 1.0f);
        byte masterGain = (byte)Math.Clamp((int)(_volume * 255), 0, 255);
        _adaptee.SetMasterGainByte(masterGain);
    }

    public float GetVolume() => _volume;
}
