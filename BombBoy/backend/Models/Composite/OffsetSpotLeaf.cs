namespace BombBoy.Models.Composite;

/// <summary>
/// Leaf 5: Targeted single-coordinate spot blast at a fixed offset from the bomb origin
/// (used to compose sniper sight-ticks, satellite pins, or custom pixel shapes).
/// </summary>
public sealed class OffsetSpotLeaf : IExplosionComponent
{
    public string Label { get; }
    public string BlastEmoji { get; }
    public string BombEmoji => "💣";
    public int OffsetX { get; }
    public int OffsetY { get; }

    public OffsetSpotLeaf(int offsetX, int offsetY, string label = "Spot", string blastEmoji = "⚡")
    {
        OffsetX = offsetX;
        OffsetY = offsetY;
        Label = label;
        BlastEmoji = blastEmoji;
    }

    public void Explode(Map map, int originX, int originY, int range)
    {
        int tx = originX + OffsetX;
        int ty = originY + OffsetY;
        if (!map.IsValidCoord(tx, ty))
            return;

        string previousEmoji = map.CurrentBlastEmoji;
        map.CurrentBlastEmoji = BlastEmoji;
        try
        {
            map.SpawnBlast(tx, ty);
        }
        finally
        {
            map.CurrentBlastEmoji = previousEmoji;
        }
    }

    public string GetPatternDescription() => Label;
}
