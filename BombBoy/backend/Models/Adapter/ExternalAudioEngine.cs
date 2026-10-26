using System;

namespace BombBoy.Models.Adapter;

/// <summary>
/// Adaptee: išorinis ar žemo lygio garso variklis / biblioteka su nesuderinama sąsaja.
/// Ji nieko nežino apie žaidimo domeną (SoundEffectType), o reikalauja fizinių failų pavadinimų,
/// aparatūrinių garso kanalų (0..15) ir baitinių stiprinimo lygių (gain: 0..255).
/// </summary>
public class ExternalAudioEngine
{
    public event Action<string, byte>? OnAudioPacketEmitted;

    public byte MasterGainByte { get; private set; } = 255;

    public void SetMasterGainByte(byte gain)
    {
        MasterGainByte = gain;
    }

    public void ExecuteSoundCommand(string soundFileName, int channel, byte gainLevel, bool loop)
    {
        // žemo lygio kanalo stiprinimo perskaičiavimas
        int computedGain = (gainLevel * MasterGainByte) / 255;
        byte finalGain = (byte)Math.Clamp(computedGain, 0, 255);

        OnAudioPacketEmitted?.Invoke(soundFileName, finalGain);
    }
}
