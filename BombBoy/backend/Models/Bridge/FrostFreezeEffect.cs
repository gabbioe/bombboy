using BombBoy.Models;

namespace BombBoy.Models.Bridge;

public class FrostFreezeEffect : IWeaponEffect
{
    public const float FreezeDurationSeconds = 2.0f;

    public string EffectName => "Frost";
    public string VisualEmoji => "❄️";

    public static void ApplyFreezeToPlayer(Player victim, float duration = FreezeDurationSeconds)
    {
        victim.ApplyFrost(duration);
    }

    public void Apply(Player user, Map map, int targetX, int targetY)
    {
        if (!map.IsValidCoord(targetX, targetY)) return;

        map.CurrentBlastOwnerId = user.Id;
        map.CurrentBlastEmoji = VisualEmoji;
        // Frost effect creates an icy snowflake blast (❄️) which freezes hit players
        map.SpawnBlast(targetX, targetY, durationTicks: 5);
        map.CurrentBlastEmoji = "💥";
        map.CurrentBlastOwnerId = null;
    }
}
