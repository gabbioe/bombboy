using BombBoy.Models;

namespace BombBoy.Models.Decorator;

public interface IPlayerComponent
{
    bool CanPassThroughWalls { get; }
    int SpeedBonus { get; }
    bool HasShield { get; }
    
    bool TakeDamage(Player player);
    void Update(float deltaTime);
    string GetStatusDescription();
}
