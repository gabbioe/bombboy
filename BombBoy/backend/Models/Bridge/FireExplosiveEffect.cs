using BombBoy.Models;

namespace BombBoy.Models.Bridge;

public class FireExplosiveEffect : IWeaponEffect
{
    public string EffectName => "Fire";
    public string VisualEmoji => "💥";

    public void Apply(Player user, Map map, int targetX, int targetY)
    {
        if (!map.IsValidCoord(targetX, targetY)) return;

        map.CurrentBlastOwnerId = user.Id;
        map.CurrentBlastEmoji = VisualEmoji;
        // Standard fire explosion: spawns blast which breaks destructible blocks and damages players
        map.SpawnBlast(targetX, targetY, durationTicks: 5);
        map.CurrentBlastEmoji = "💥";
        map.CurrentBlastOwnerId = null;
    }
}
