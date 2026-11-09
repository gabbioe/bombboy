using BombBoy.Models;

namespace BombBoy.Models.Bridge;

public class FrostFreezeEffect : IWeaponEffect
{
    public string EffectName => "Frost";
    public string VisualEmoji => "❄️";

    public void Apply(Player user, Map map, int targetX, int targetY)
    {
        if (!map.IsValidCoord(targetX, targetY)) return;

        map.CurrentBlastOwnerId = user.Id;
        map.CurrentBlastEmoji = VisualEmoji;
        // Frost effect creates an icy snowflake blast (❄️)
        map.SpawnBlast(targetX, targetY, durationTicks: 5);
        map.CurrentBlastEmoji = "💥";
        map.CurrentBlastOwnerId = null;
    }
}
