using System;
using System.Linq;
using BombBoy.Models.Decorator;

namespace BombBoy.Models.Mediator;

/// <summary>
/// 'ConcreteColleague2': Hit-Bounty Coordinator colleague.
/// Processes '!bounty' requests routed by the Mediator, tracks the hunted target player,
/// and grants +1 Life &amp; a 10s Shield when the hunted target is eliminated.
/// </summary>
public sealed class BountyMasterColleague : ChatColleague
{
    private readonly Match _match;

    public string? ActiveTargetId { get; private set; }
    public string? ActiveTargetName { get; private set; }
    public string? PlacedByName { get; private set; }

    public BountyMasterColleague(AbstractChatMediator mediator, Match match)
        : base(mediator, "colleague-bounty", "BountyMaster")
    {
        _match = match ?? throw new ArgumentNullException(nameof(match));
    }

    public override void Notify(string senderName, string message, string channel)
    {
        string arg = message.Length > "/bounty".Length
            ? message["/bounty".Length..].Trim().TrimStart('@')
            : string.Empty;

        var aliveCandidates = _match.GetAlivePlayers()
            .Where(p => !p.Name.Equals(senderName, StringComparison.OrdinalIgnoreCase))
            .ToList();

        if (aliveCandidates.Count == 0)
        {
            Send($"No valid alive opponents available to place a bounty on.");
            return;
        }

        Player? target = string.IsNullOrWhiteSpace(arg)
            ? aliveCandidates[Random.Shared.Next(aliveCandidates.Count)]
            : aliveCandidates.FirstOrDefault(p => p.Name.Contains(arg, StringComparison.OrdinalIgnoreCase));

        if (target == null)
        {
            Send($"Could not find alive player matching '{arg}'.");
            return;
        }

        ActiveTargetId = target.Id;
        ActiveTargetName = target.Name;
        PlacedByName = senderName;

        Send($"🎯 {senderName} placed a HIT BOUNTY on {target.Name}! Eliminate them for +1 Life & 10s Shield!");
    }

    public void CheckBountyClaimed(Player victim, Player? killer)
    {
        if (ActiveTargetId == null || victim.Id != ActiveTargetId)
            return;

        string huntedName = ActiveTargetName ?? victim.Name;
        ActiveTargetId = null;
        ActiveTargetName = null;
        PlacedByName = null;

        if (killer != null && killer.Id != victim.Id && killer.IsAlive)
        {
            killer.Lives = Math.Min(killer.Lives + 1, 5);
            killer.Decorate(c => new ImmortalDecorator(c, 10f));
            killer.ShieldDuration = 10;
            Send($"💰 {killer.Name} claimed the bounty on {huntedName}! Rewarded +1 ❤️ & 10s 🛡 Shield!");
        }
        else
        {
            Send($"💀 Bounty target {huntedName} perished without a killer — bounty expired.");
        }
    }

    public void ResetBounty()
    {
        ActiveTargetId = null;
        ActiveTargetName = null;
        PlacedByName = null;
    }
}
