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
        // Queue stores: (X coordinate, Y coordinate, current distance from origin)
        var queue = new Queue<(int x, int y, int dist)>();
        var visited = new HashSet<(int, int)>();

        // Start at the bomb's origin
        queue.Enqueue((originX, originY, 0));
        visited.Add((originX, originY));

        while (queue.Count > 0)
        {
            var (currX, currY, dist) = queue.Dequeue();

            // 1. Damage and spawn blast on the current cell
            map.SpawnBlast(currX, currY);

            // 2. If we haven't hit the maximum range, check neighbors for spreading
            if (dist < range)
            {
                var currentTile = map.GetTile(currX, currY);

                // Stop propagation further down this branch if we hit a breakable block
                if ((currX != originX || currY != originY) && currentTile is DestructableTile)
                {
                    continue;
                }

                // Spread to adjacent cells
                foreach (var (dx, dy) in Directions)
                {
                    int nextX = currX + dx;
                    int nextY = currY + dy;

                    if (map.IsValidCoord(nextX, nextY) && !visited.Contains((nextX, nextY)))
                    {
                        var nextTile = map.GetTile(nextX, nextY);

                        // Stop propagation if next tile is indestructible
                        if (nextTile.Type == "IndestructableTile")
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