using System;
using BombBoy.Models.Adapter;

namespace BombBoy.Models.Mediator;

/// <summary>
/// 'ConcreteColleague4': Sound Alert colleague.
/// Triggers audio notification cues through ISoundEffectService when notified by the Mediator
/// (e.g., on whispers or bounty alerts) without chat/bounty colleagues knowing about audio.
/// </summary>
public sealed class SoundCueColleague : ChatColleague
{
    private readonly ISoundEffectService _soundService;

    public SoundCueColleague(AbstractChatMediator mediator, ISoundEffectService soundService)
        : base(mediator, "colleague-sound", "SoundCue")
    {
        _soundService = soundService ?? throw new ArgumentNullException(nameof(soundService));
    }

    public override void Notify(string senderName, string message, string channel)
    {
        if (channel == "Bounty")
        {
            _soundService.PlaySound(SoundEffectType.ItemAppear);
        }
        else if (channel == "Whisper")
        {
            _soundService.PlaySound(SoundEffectType.ButtonClick);
        }
    }
}
