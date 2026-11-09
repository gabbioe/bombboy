using BombBoy.Models;

namespace BombBoy.Models.Decorator;

public class BasicPlayerComponent : IPlayerComponent
{
    public virtual bool CanPassThroughWalls => false;
    public virtual int SpeedBonus => 0;
    public virtual bool HasShield => false;

    public virtual bool TakeDamage(Player player)
    {
        player.Lives--;
        if (player.Lives <= 0)
        {
            player.Lives = 0;
            player.IsAlive = false;
            return true;
        }
        return false;
    }

    public virtual void Update(float deltaTime)
    {
        // Base player component has no ticking effects
    }

    public virtual string GetStatusDescription() => "Normal";
}
