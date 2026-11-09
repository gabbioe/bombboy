using BombBoy.Models;

namespace BombBoy.Models.Bridge;

public interface IWeaponEffect
{
    string EffectName { get; }
    string VisualEmoji { get; }
    void Apply(Player user, Map map, int targetX, int targetY);
}
