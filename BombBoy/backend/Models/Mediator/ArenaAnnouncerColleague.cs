using System;
using BombBoy.Services;

namespace BombBoy.Models.Mediator;

/// <summary>
/// 'ConcreteColleague3': Arena Announcer &amp; Telemetry Bot colleague.
/// Responds to '!status' queries routed by the Mediator and broadcasts round milestones.
/// </summary>
public sealed class ArenaAnnouncerColleague : ChatColleague
{
    private readonly Match _match;
    private readonly RoundService _round;

    public ArenaAnnouncerColleague(AbstractChatMediator mediator, Match match, RoundService round)
        : base(mediator, "colleague-announcer", "Announcer")
    {
        _match = match ?? throw new ArgumentNullException(nameof(match));
        _round = round ?? throw new ArgumentNullException(nameof(round));
    }

    public override void Notify(string senderName, string message, string channel)
    {
        int alive = _match.GetAlivePlayers().Count;
        int total = _match.GetAllPlayersList().Count;
        int secs = (int)Math.Ceiling(_round.RemainingSeconds);

        Send($"📊 Stage: {_round.Stage} | Round {_round.CurrentRound}/{_round.TotalRounds} | Alive: {alive}/{total} | Timer: {secs}s");
    }

    public void AnnounceRoundStart(int roundNumber, string mapType)
    {
        Send($"🔔 Round {roundNumber} started on {mapType.ToUpperInvariant()} map! Chat freely or use '/@name msg', '/bounty', '/status', '/help'.");
    }

    public void AnnounceRoundEnd(string winnerName, int roundNumber)
    {
        Send($"🏁 Round {roundNumber} ended — Winner: {winnerName}!");
    }
}
