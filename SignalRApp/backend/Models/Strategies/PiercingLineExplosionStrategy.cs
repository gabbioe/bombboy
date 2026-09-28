using SignalRApp.Models.GridEntities;

namespace SignalRApp.Models.Strategies;

/// <summary>
/// Piercing Line explosion strategy.
/// Raycasts in cardinal directions and pierces through breakable blocks without stopping,
/// only blocked by indestructible walls.
/// </summary>
public class PiercingLineExplosionStrategy : IExplosionStrategy
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

        // 2. Piercing raycasting
        foreach (var (dx, dy) in Directions)
        {
            for (int r = 1; r <= range; r++)
            {
                int targetX = originX + (dx * r);
                int targetY = originY + (dy * r);

                if (!map.IsValidCoord(targetX, targetY))
                    break;

                var cell = map.GetCell(targetX, targetY);

                // Indestructible blocks still stop the piercing ray
                if (cell is Block block && !block.Is_Breakable)
                    break;

                // Spawn blast and break block, but DO NOT break loop - continue piercing!
                map.SpawnBlast(targetX, targetY);
            }
        }
    }
}
