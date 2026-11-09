using BombBoy.Models;

namespace BombBoy.Models.Decorator;

public abstract class PlayerDecorator : IPlayerComponent
{
    protected IPlayerComponent _wrapped;

    public PlayerDecorator(IPlayerComponent wrapped)
    {
        _wrapped = wrapped ?? throw new System.ArgumentNullException(nameof(wrapped));
    }

    public virtual bool CanPassThroughWalls => _wrapped.CanPassThroughWalls;
    public virtual int SpeedBonus => _wrapped.SpeedBonus;
    public virtual bool HasShield => _wrapped.HasShield;

    public virtual bool TakeDamage(Player player)
    {
        return _wrapped.TakeDamage(player);
    }

    public virtual void Update(float deltaTime)
    {
        _wrapped.Update(deltaTime);
    }

    public virtual string GetStatusDescription()
    {
        return _wrapped.GetStatusDescription();
    }

    public IPlayerComponent GetWrapped() => _wrapped;
}
