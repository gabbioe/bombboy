using SignalRApp.Models.GridEntities;

namespace SignalRApp.Models.Strategies;

public class ClassicCrossExplosionStrategy : IExplosionStrategy
{
    private static readonly (int dx, int dy)[] Directions = 
    {
        (0, -1), // Up
        (0, 1),  // Down
        (-1, 0), // Left
        (1, 0)   // Right
    };

    public void Explode(Map map, int originX, int originY, int range)
    {
        // 1. Center explosion
        map.SpawnBlast(originX, originY);

        // 2. Raycast in all 4 cardinal directions
        foreach (var (dx, dy) in Directions)
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