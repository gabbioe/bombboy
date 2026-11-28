using System;

namespace BombBoy.Models.Composite;

/// <summary>
/// Leaf 2: Hollow Perimeter Ring shape.
/// Spawns blasts ONLY along the outer square perimeter at distance R from origin
/// (leaving inner tiles untouched to create bullseyes and shockwave halos).
/// </summary>
public sealed class HollowRingLeaf : IExplosionComponent
{
    public string Label { get; }
    public string BlastEmoji { get; }
    public string BombEmoji => "💣";
    public int BaseRadius { get; }
    public bool ScaleWithRange { get; }

    public HollowRingLeaf(
        string label = "HollowRing",
        string blastEmoji = "🔮",
        int baseRadius = 2,
        bool scaleWithRange = true)
    {
        Label = label;
        BlastEmoji = blastEmoji;
        BaseRadius = Math.Max(0, baseRadius);
        ScaleWithRange = scaleWithRange;
    }

    public void Explode(Map map, int originX, int originY, int range)
    {
        int radius = ScaleWithRange ? BaseRadius + Math.Max(0, range - 1) : BaseRadius;
        string previousEmoji = map.CurrentBlastEmoji;
        map.CurrentBlastEmoji = BlastEmoji;

        try
        {
            if (radius == 0)
            {
                map.SpawnBlast(originX, originY);
                return;
            }

            for (int dx = -radius; dx <= radius; dx++)
            {
                for (int dy = -radius; dy <= radius; dy++)
                {
                    if (Math.Max(Math.Abs(dx), Math.Abs(dy)) != radius)
                        continue;

                    int tx = originX + dx;
                    int ty = originY + dy;
                    if (map.IsValidCoord(tx, ty))
                    {
                        map.SpawnBlast(tx, ty);
                    }
                }
            }
        }
        finally
        {
            map.CurrentBlastEmoji = previousEmoji;
        }
    }

    public string GetPatternDescription() => Label;
}
