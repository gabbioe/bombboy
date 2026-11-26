namespace BombBoy.Models.Mediator;

/// <summary>
/// The 'Mediator' abstract class participant in the DoFactory Mediator pattern.
/// Defines the contract for communication between ChatColleague objects.
/// </summary>
public abstract class AbstractChatMediator
{
    public abstract void Register(ChatColleague colleague);
    public abstract void Unregister(string colleagueId);
    public abstract void Send(string message, ChatColleague colleague);
}
