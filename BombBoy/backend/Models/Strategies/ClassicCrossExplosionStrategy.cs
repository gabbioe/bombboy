using BombBoy.Models.Tiles;

namespace BombBoy.Models.Strategies;

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

                var tile = map.GetTile(targetX, targetY);

                // Indestructible blocks stop the ray
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