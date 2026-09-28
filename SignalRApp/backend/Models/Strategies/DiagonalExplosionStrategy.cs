using SignalRApp.Models.GridEntities;

namespace SignalRApp.Models.Strategies;

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

                var cell = map.GetCell(targetX, targetY);

                // Stop at indestructible blocks
                if (cell is Block block && !block.Is_Breakable)
                    break;

                bool isBreakableBlock = cell is Block;

                map.SpawnBlast(targetX, targetY);

                // Ray stops after hitting a breakable block
                if (isBreakableBlock)
                    break;
            }
        }
    }
}
