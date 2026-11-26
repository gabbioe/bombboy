using System;

namespace BombBoy.Models.Mediator;

/// <summary>
/// 'ConcreteColleague1': Represents a connected human Player in the mediated chatroom.
/// Sends public chat, '@Player' whispers, Ghost chat (when eliminated), and '!bounty'/'!status' requests.
/// </summary>
public sealed class PlayerChatColleague : ChatColleague
{
    private readonly Action<string, string, string, string> _deliverToClient;

    public Player Player { get; }
    public bool IsAlive => Player.IsAlive;

    public PlayerChatColleague(
        AbstractChatMediator mediator,
        Player player,
        Action<string, string, string, string> deliverToClient)
        : base(mediator, player.Id, player.Name)
    {
        Player = player ?? throw new ArgumentNullException(nameof(player));
        _deliverToClient = deliverToClient ?? throw new ArgumentNullException(nameof(deliverToClient));
    }

    public override void Notify(string senderName, string message, string channel)
    {
        _deliverToClient(Id, senderName, message, channel);
    }
}
