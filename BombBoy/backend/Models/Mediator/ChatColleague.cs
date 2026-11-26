using System;

namespace BombBoy.Models.Mediator;

/// <summary>
/// The 'Colleague' abstract class participant in the DoFactory Mediator pattern.
/// Each colleague holds a reference only to its AbstractChatMediator, never to other colleagues directly.
/// </summary>
public abstract class ChatColleague
{
    protected AbstractChatMediator mediator;

    public string Id { get; }
    public string Name { get; }

    protected ChatColleague(AbstractChatMediator mediator, string id, string name)
    {
        this.mediator = mediator ?? throw new ArgumentNullException(nameof(mediator));
        Id = id ?? throw new ArgumentNullException(nameof(id));
        Name = name ?? throw new ArgumentNullException(nameof(name));
    }

    public virtual void Send(string message)
    {
        mediator.Send(message, this);
    }

    public abstract void Notify(string senderName, string message, string channel);
}
