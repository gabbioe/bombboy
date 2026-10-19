using System;
using BombBoy.Models.Tiles;

namespace BombBoy.Models.Strategies;

/// <summary>
/// Area Box / Cluster explosion strategy.
/// Explodes in a square area (e.g. 3x3) around the bomb.
/// </summary>
public class AreaBoxExplosionStrategy : IExplosionStrategy
{
    public void Explode(Map map, int originX, int originY, int range)
    {
        // Radius is capped between 1 and range (e.g. 1 gives a 3x3 box)
        int radius = Math.Max(1, Math.Min(range, 2));

        for (int dx = -radius; dx <= radius; dx++)
        {
            for (int dy = -radius; dy <= radius; dy++)
            {
                int targetX = originX + dx;
                int targetY = originY + dy;

                if (!map.IsValidCoord(targetX, targetY))
                    continue;

                var tile = map.GetTile(targetX, targetY);

                // Skip indestructible blocks
                if (tile.Type == "IndestructableTile")
                    continue;

                map.SpawnBlast(targetX, targetY);
            }
        }
    }
}
