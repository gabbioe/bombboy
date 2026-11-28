using System;

namespace BombBoy.Models.Composite;

/// <summary>
/// Leaf 4: Alternating Checkerboard Grid shape.
/// Explodes only on alternating parity tiles ((|dx| + |dy|) % 2 == Parity) within a square zone,
/// creating a woven chessboard pattern with safe pockets between blasts.
/// </summary>
public sealed class CheckerboardLeaf : IExplosionComponent
{
    public string Label { get; }
    public string BlastEmoji { get; }
    public string BombEmoji => "💣";
    public int BaseRadius { get; }
    public int Parity { get; }

    public CheckerboardLeaf(
        string label = "Checkerboard",
        string blastEmoji = "☢️",
        int baseRadius = 2,
        int parity = 0)
    {
        Label = label;
        BlastEmoji = blastEmoji;
        BaseRadius = Math.Max(1, baseRadius);
        Parity = Math.Abs(parity) % 2;
    }

    public void Explode(Map map, int originX, int originY, int range)
    {
        int radius = Math.Min(4, BaseRadius + Math.Max(0, range - 1));
        string previousEmoji = map.CurrentBlastEmoji;
        map.CurrentBlastEmoji = BlastEmoji;

        try
        {
            for (int dx = -radius; dx <= radius; dx++)
            {
                for (int dy = -radius; dy <= radius; dy++)
                {
                    if ((Math.Abs(dx) + Math.Abs(dy)) % 2 != Parity)
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
