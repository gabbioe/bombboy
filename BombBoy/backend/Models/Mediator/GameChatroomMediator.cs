using System;
using System.Collections.Concurrent;
using System.Linq;
using BombBoy.Services;

namespace BombBoy.Models.Mediator;

/// <summary>
/// The 'ConcreteMediator' participant in the DoFactory Mediator pattern.
/// Coordinates interaction across 4 distinct colleague classes:
///   1. PlayerChatColleague (human players: public chat, whispers, dead-only Ghost chat)
///   2. BountyMasterColleague ('!bounty' hit-contracts and elimination rewards)
///   3. ArenaAnnouncerColleague ('!status' queries and round announcements)
///   4. SoundCueColleague (audio alert cues on whispers and bounties)
/// </summary>
public sealed class GameChatroomMediator : AbstractChatMediator
{
    private readonly ConcurrentDictionary<string, PlayerChatColleague> _players = new();
    private readonly Func<GameStage> _getCurrentStage;

    public BountyMasterColleague? BountyColleague { get; set; }
    public ArenaAnnouncerColleague? AnnouncerColleague { get; set; }
    public SoundCueColleague? SoundColleague { get; set; }

    public GameChatroomMediator(Func<GameStage> getCurrentStage)
    {
        _getCurrentStage = getCurrentStage ?? throw new ArgumentNullException(nameof(getCurrentStage));
    }

    public override void Register(ChatColleague colleague)
    {
        switch (colleague)
        {
            case PlayerChatColleague playerColleague:
                _players[playerColleague.Id] = playerColleague;
                break;
            case BountyMasterColleague bounty:
                BountyColleague = bounty;
                break;
            case ArenaAnnouncerColleague announcer:
                AnnouncerColleague = announcer;
                break;
            case SoundCueColleague sound:
                SoundColleague = sound;
                break;
        }
    }

    public override void Unregister(string colleagueId)
    {
        _players.TryRemove(colleagueId, out _);
    }

    public PlayerChatColleague? GetPlayerColleague(string connectionId) =>
        _players.GetValueOrDefault(connectionId);

    public override void Send(string message, ChatColleague colleague)
    {
        if (string.IsNullOrWhiteSpace(message))
            return;

        string trimmed = message.Trim();

        // 1. Messages originating from BountyMasterColleague -> broadcast on Bounty channel + trigger SFX
        if (colleague is BountyMasterColleague)
        {
            SoundColleague?.Notify(colleague.Name, trimmed, "Bounty");
            BroadcastToAllPlayers(colleague.Name, trimmed, "Bounty");
            return;
        }

        // 2. Messages originating from ArenaAnnouncerColleague -> broadcast on Announcer channel
        if (colleague is ArenaAnnouncerColleague)
        {
            BroadcastToAllPlayers(colleague.Name, trimmed, "Announcer");
            return;
        }

        // 3. Messages originating from a PlayerChatColleague
        if (colleague is PlayerChatColleague sender)
        {
            // 3a. '!bounty [target]' -> route to BountyMasterColleague
            if (trimmed.StartsWith("!bounty", StringComparison.OrdinalIgnoreCase))
            {
                if (_getCurrentStage() != GameStage.InGame)
                {
                    sender.Notify("System", "Bounties can only be placed while a round is InGame.", "System");
                    return;
                }
                BountyColleague?.Notify(sender.Name, trimmed, "Bounty");
                return;
            }

            // 3b. '!status' -> route to ArenaAnnouncerColleague
            if (trimmed.Equals("!status", StringComparison.OrdinalIgnoreCase))
            {
                AnnouncerColleague?.Notify(sender.Name, trimmed, "Announcer");
                return;
            }

            // 3c. '@PlayerName secret' -> private Whisper between two PlayerChatColleagues + SoundCueColleague
            if (trimmed.StartsWith('@'))
            {
                HandleWhisper(sender, trimmed);
                return;
            }

            // 3d. Eliminated players during InGame talk in Ghost Chat (isolated to dead players only!)
            bool isGhostChat = _getCurrentStage() == GameStage.InGame && !sender.IsAlive;
            if (isGhostChat)
            {
                foreach (var p in _players.Values.Where(p => !p.IsAlive))
                {
                    p.Notify(sender.Name, trimmed, "Ghost");
                }
                return;
            }

            // 3e. Standard Public Chat -> delivered to all registered PlayerChatColleagues
            BroadcastToAllPlayers(sender.Name, trimmed, "Public");
        }
    }

    private void HandleWhisper(PlayerChatColleague sender, string trimmed)
    {
        int firstSpace = trimmed.IndexOf(' ');
        if (firstSpace <= 1 || firstSpace >= trimmed.Length - 1)
        {
            sender.Notify("System", "Usage: @PlayerName <message>", "System");
            return;
        }

        string targetQuery = trimmed[1..firstSpace];
        string body = trimmed[(firstSpace + 1)..].Trim();

        var recipient = _players.Values.FirstOrDefault(p =>
            p.Name.Contains(targetQuery, StringComparison.OrdinalIgnoreCase));

        if (recipient == null)
        {
            sender.Notify("System", $"No player matching '{targetQuery}' found.", "System");
            return;
        }

        // Prevent dead players from whispering spoilers to alive players mid-round
        if (_getCurrentStage() == GameStage.InGame && !sender.IsAlive && recipient.IsAlive)
        {
            sender.Notify("System", "Eliminated ghosts cannot whisper alive players during a round!", "Ghost");
            return;
        }

        SoundColleague?.Notify(sender.Name, body, "Whisper");
        recipient.Notify($"{sender.Name} → You", body, "Whisper");
        if (recipient.Id != sender.Id)
        {
            sender.Notify($"You → {recipient.Name}", body, "Whisper");
        }
    }

    private void BroadcastToAllPlayers(string senderName, string message, string channel)
    {
        foreach (var playerColleague in _players.Values)
        {
            playerColleague.Notify(senderName, message, channel);
        }
    }
}
