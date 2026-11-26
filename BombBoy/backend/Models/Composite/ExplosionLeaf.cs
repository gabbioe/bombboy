using System;
using BombBoy.Models.Strategies;

namespace BombBoy.Models.Composite;

/// <summary>
/// Leaf 1: Adapts a stateless Flyweight <see cref="IExplosionStrategy"/> with extrinsic
/// blast color/emoji, range modifier, and coordinate offset.
/// </summary>
public sealed class ExplosionLeaf : IExplosionComponent
{
    public IExplosionStrategy InnerStrategy { get; }
    public string Label { get; }
    public string BlastEmoji { get; }
    public string BombEmoji => "💣";
    public int RangeBonus { get; }
    public int OffsetX { get; }
    public int OffsetY { get; }

    public ExplosionLeaf(
        IExplosionStrategy innerStrategy,
        string? label = null,
        string blastEmoji = "💥",
        int rangeBonus = 0,
        int offsetX = 0,
        int offsetY = 0)
    {
        InnerStrategy = innerStrategy ?? throw new ArgumentNullException(nameof(innerStrategy));
        Label = label ?? IExplosionComponent.Describe(innerStrategy);
        BlastEmoji = blastEmoji;
        RangeBonus = rangeBonus;
        OffsetX = offsetX;
        OffsetY = offsetY;
    }

    public void Explode(Map map, int originX, int originY, int range)
    {
        int targetOriginX = originX + OffsetX;
        int targetOriginY = originY + OffsetY;

        if (!map.IsValidCoord(targetOriginX, targetOriginY))
            return;

        var originTile = map.GetTile(targetOriginX, targetOriginY);
        if (originTile == null || originTile.Type == "IndestructableTile")
            return;

        int effectiveRange = Math.Max(1, range + RangeBonus);
        string previousEmoji = map.CurrentBlastEmoji;
        map.CurrentBlastEmoji = BlastEmoji;
        try
        {
            InnerStrategy.Explode(map, targetOriginX, targetOriginY, effectiveRange);
        }
        finally
        {
            map.CurrentBlastEmoji = previousEmoji;
        }
    }

    public string GetPatternDescription() => Label;
}
