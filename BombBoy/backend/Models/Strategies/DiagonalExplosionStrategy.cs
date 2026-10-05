using BombBoy.Models.Tiles;

namespace BombBoy.Models.Strategies;

/// <summary>
/// Diagonal (X-shape) explosion strategy.
/// Projects blasts diagonally, reaching behind straight barriers.
/// </summary>
public class DiagonalExplosionStrategy : IExplosionStrategy
{
    private static readonly (int dx, int dy)[] DiagonalDirections = 
    {
        (1, 1),   // Down-Right
        (1, -1),  // Up-Right
        (-1, 1),  // Down-Left
        (-1, -1)  // Up-Left
    };

    public void Explode(Map map, int originX, int originY, int range)
    {
        // 1. Center explosion
        map.SpawnBlast(originX, originY);

        // 2. Diagonal raycasting
        foreach (var (dx, dy) in DiagonalDirections)
        {
            for (int r = 1; r <= range; r++)
            {
                int targetX = originX + (dx * r);
                int targetY = originY + (dy * r);

                if (!map.IsValidCoord(targetX, targetY))
                    break;

                var tile = map.GetTile(targetX, targetY);

                // Stop at indestructible blocks
                if (tile is IndestructableTile)
                    break;

                bool isDestructible = tile is DestructableTile;

                map.SpawnBlast(targetX, targetY);

                // Ray stops after hitting a breakable block
                if (isDestructible)
                    break;
            }
        }
    }
}
