using System;

namespace BombBoy.Models.Composite;

/// <summary>
/// Leaf 3: Chess-Knight L-Jump Star shape.
/// Hits all 8 chess-knight offsets ((+/-1, +/-2) and (+/-2, +/-1)) around the origin,
/// curving around indestructible 2x2 map pillars that block straight and diagonal rays.
/// </summary>
public sealed class KnightScatterLeaf : IExplosionComponent
{
    private static readonly (int dx, int dy)[] KnightOffsets =
    {
        (1, 2), (2, 1), (-1, 2), (-2, 1),
        (1, -2), (2, -1), (-1, -2), (-2, -1)
    };

    public string Label { get; }
    public string BlastEmoji { get; }
    public string BombEmoji => "💣";

    public KnightScatterLeaf(string label = "KnightStar", string blastEmoji = "⚡")
    {
        Label = label;
        BlastEmoji = blastEmoji;
    }

    public void Explode(Map map, int originX, int originY, int range)
    {
        int maxSteps = range >= 3 ? 2 : 1;
        string previousEmoji = map.CurrentBlastEmoji;
        map.CurrentBlastEmoji = BlastEmoji;

        try
        {
            for (int step = 1; step <= maxSteps; step++)
            {
                foreach (var (ox, oy) in KnightOffsets)
                {
                    int tx = originX + (ox * step);
                    int ty = originY + (oy * step);
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
