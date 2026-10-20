using System.Collections.Generic;
using BombBoy.Models.Tiles;

namespace BombBoy.Models.Strategies;

public class FloodFillExplosionStrategy : IExplosionStrategy
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
        var queue = new Queue<(int x, int y, int dist)>();
        var visited = new HashSet<(int, int)>();

        queue.Enqueue((originX, originY, 0));
        visited.Add((originX, originY));

        while (queue.Count > 0)
        {
            var (currX, currY, dist) = queue.Dequeue();

            var currentTile = map.GetTile(currX, currY);
            if (currentTile == null || currentTile.Type == "IndestructableTile")
                continue;

            bool isDestructible = currentTile is DestructableTile;

            // 1. Damage and spawn blast on the current cell
            map.SpawnBlast(currX, currY);

            // 2. Breakable blocks stop propagation further down this branch
            if (isDestructible && (currX != originX || currY != originY))
            {
                continue;
            }

            // 3. If we haven't hit the maximum range, spread to adjacent cells
            if (dist < range)
            {
                foreach (var (dx, dy) in Directions)
                {
                    int nextX = currX + dx;
                    int nextY = currY + dy;

                    if (map.IsValidCoord(nextX, nextY) && !visited.Contains((nextX, nextY)))
                    {
                        var nextTile = map.GetTile(nextX, nextY);
                        if (nextTile == null || nextTile.Type == "IndestructableTile")
                        {
                            continue;
                        }

                        visited.Add((nextX, nextY));
                        queue.Enqueue((nextX, nextY, dist + 1));
                    }
                }
            }
        }
    }
}