using Microsoft.AspNetCore.Authorization;
using SignalRApp.Models;
using SignalRApp.Models.GridEntities;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text.Json;

namespace SignalRApp.Models;

public class Map
{
    public int Size_X { get; private set; }
    public int Size_Y { get; private set; }

    private Cell[,] MapGrid = null!;
    private readonly Dictionary<string, (int x, int y)> PlayerLocations = new();
    private readonly Dictionary<string, PlayerCell> _playerCells = new();
    private readonly List<(int x, int y, Blast blast)> _activeBlasts = new();

    private int PlayerCount = 0;
    private readonly Stack<int[]> AvailableStartingPositions = new();

    /// <summary>
    /// Generates the map
    /// </summary>
    public Map([Range(5, int.MaxValue)] int size_X, [Range(5, int.MaxValue)] int size_Y, int BlockSpawnChance)
    {
        if (size_X < 5 || size_Y < 5)
        {
            throw new ArgumentException($"Both Size X ({size_X}) and Size Y ({size_Y}) must be at least 5.");
        }

        if (size_X % 2 == 0 || size_Y % 2 == 0)
        {
            throw new ArgumentException($"Both Size X ({size_X}) and Size Y ({size_Y}) must NOT be even.");
        }

        // del borderiu +2 
        Size_X = size_X + 2;
        Size_Y = size_Y + 2;

        AvailableStartingPositions.Push([Size_X - 2, Size_Y - 2]);
        AvailableStartingPositions.Push([Size_X - 2, 1]);
        AvailableStartingPositions.Push([1, Size_Y - 2]);
        AvailableStartingPositions.Push([1, 1]);

        generateMapGood(BlockSpawnChance);
    }

    private void generateMapGood(int BlockSpawnChance)
    {
        Random rand = new Random();
        MapGrid = new Cell[Size_X, Size_Y];
        for (int i = 0; i < Size_X; i++)
        {
            for (int j = 0; j < Size_Y; j++)
            {
                // borderiai
                if (i == 0 || i == Size_X - 1 || j == 0 || j == Size_Y - 1)
                    MapGrid[i, j] = new Block(false);
                // kas antra bloka pazymi kad butu 
                else if (i % 2 == 0 && j % 2 == 0)
                    MapGrid[i, j] = new Block(false);
                // Tikrina ar nera kampuose kur playeriai spawninasi 
                else if ((i > 2 || j > 2) && (i < Size_X - 3 || j > 2) && (i > 2 || j < Size_Y - 3) && (i < Size_X - 3 || j < Size_Y - 3))
                {
                    if (rand.Next(0, 10) < BlockSpawnChance)
                        MapGrid[i, j] = new Block(true);
                    else
                        MapGrid[i, j] = new Air("Air");
                }
                else
                {
                    MapGrid[i, j] = new Air("Air");
                }
            }
        }
    }

    public int[] AddPlayer(Player newPlayer)
    {
        if (PlayerCount < 4 && AvailableStartingPositions.Count > 0)
        {
            int[] playerLocation = AvailableStartingPositions.Pop();
            PlayerCount++;
            PlayerLocations[newPlayer.Id] = (playerLocation[0], playerLocation[1]);

            var pCell = new PlayerCell(newPlayer, newPlayer.Id);
            _playerCells[newPlayer.Id] = pCell;
            MapGrid[playerLocation[0], playerLocation[1]] = pCell;

            return playerLocation;
        }

        throw new ArgumentException("Game only supports up to 4 players");
    }

    public bool RemovePlayer(string playerId)
    {
        if (PlayerLocations.TryGetValue(playerId, out var player))
        {
            PlayerLocations.Remove(playerId);
            _playerCells.Remove(playerId);
            if (MapGrid[player.x, player.y] is PlayerCell)
            {
                MapGrid[player.x, player.y] = new Air();
            }
            AvailableStartingPositions.Push([player.x, player.y]);
            PlayerCount = Math.Max(0, PlayerCount - 1);
            return true;
        }
        return false;
    }

    public bool CheckIfCanMove(int x, int y)
    {
        return checkIfCanMove(x, y);
    }

    private bool checkIfCanMove(int x, int y)
    {
        if (!IsValidCoord(x, y)) return false;
        return MapGrid[x, y].CanWalk();
    }

    public bool Move(int dx, int dy, string PlayerId)
    {
        if (PlayerLocations.TryGetValue(PlayerId, out var player))
        {
            if (Math.Abs(dx) + Math.Abs(dy) < 2)
            {
                int x = player.x;
                int y = player.y;
                int newx = x + dx;
                int newy = y + dy;

                if (checkIfCanMove(newx, newy))
                {
                    _playerCells.TryGetValue(PlayerId, out var pCell);

                    // If current tile is PlayerCell, clear it. If it was Bomb, leave Bomb intact!
                    if (MapGrid[x, y] is PlayerCell)
                    {
                        MapGrid[x, y] = new Air();
                    }

                    // If moving into Blast, damage player
                    if (MapGrid[newx, newy] is Blast)
                    {
                        pCell?.DoDamage();
                    }
                    else if (pCell != null)
                    {
                        MapGrid[newx, newy] = pCell;
                    }

                    player.x += dx;
                    player.y += dy;
                    PlayerLocations[PlayerId] = player;
                    return true;
                }
                return false;
            }
            throw new ArgumentException($"Player can only move in one direction at once, either dx({dx}) or dy({dy}) MUST be zero");
        }
        throw new ArgumentException($"Player with id [{PlayerId}] Doesn't exist");
    }

    public bool TryPlaceBomb(int x, int y, Bomb bomb)
    {
        if (!IsValidCoord(x, y)) return false;

        var cell = MapGrid[x, y];
        // Can place bomb if cell is Air or PlayerCell (not existing Bomb or Block)
        if (cell is Air || cell is PlayerCell)
        {
            MapGrid[x, y] = bomb;
            return true;
        }
        return false;
    }

    public void SpawnBlast(int x, int y, int durationTicks = 10)
    {
        if (!IsValidCoord(x, y)) return;

        DoDamange(x, y);

        var cell = MapGrid[x, y];
        if (cell is Air || cell is PlayerCell || cell is Blast || cell is Bomb)
        {
            var blast = new Blast(durationTicks);
            MapGrid[x, y] = blast;
            _activeBlasts.Add((x, y, blast));
        }
    }

    public bool UpdateBlasts()
    {
        bool changed = false;
        for (int i = _activeBlasts.Count - 1; i >= 0; i--)
        {
            var (bx, by, blast) = _activeBlasts[i];
            blast.DurationTicks--;
            if (blast.DurationTicks <= 0)
            {
                if (MapGrid[bx, by] == blast)
                {
                    MapGrid[bx, by] = new Air();
                    changed = true;
                }
                _activeBlasts.RemoveAt(i);
            }
        }
        return changed;
    }

    public void DoDamage(int x, int y) => DoDamange(x, y);

    public void DoDamange(int x, int y)
    {
        if (!IsValidCoord(x, y)) return;

        // Damage player at location if present
        foreach (var (pid, loc) in PlayerLocations)
        {
            if (loc.x == x && loc.y == y)
            {
                if (_playerCells.TryGetValue(pid, out var pCell))
                {
                    pCell.DoDamage();
                }
            }
        }

        var result = MapGrid[x, y].DoDamage();
        if (result != null)
        {
            MapGrid[x, y] = result;
        }
    }

    public Cell? GetCell(int targetX, int targetY)
    {
        if (!IsValidCoord(targetX, targetY))
        {
            return null;
        }
        return MapGrid[targetX, targetY];
    }

    public void SetCell(int x, int y, Cell cell)
    {
        if (IsValidCoord(x, y))
        {
            MapGrid[x, y] = cell;
        }
    }

    public bool IsValidCoord(int targetX, int targetY)
    {
        return targetX >= 0 && targetX < Size_X && targetY >= 0 && targetY < Size_Y;
    }

    public string ReturnMapToConsole()
    {
        string result = string.Empty;

        for (int i = 0; i < Size_X; i++)
        {
            for (int j = 0; j < Size_Y; j++)
            {
                result += MapGrid[i, j];
            }
            result += "\n";
        }
        return result;
    }

    public string ReturnMapToJSon()
    {
        var gridArray = new object[Size_X][];

        for (int i = 0; i < Size_X; i++)
        {
            gridArray[i] = new object[Size_Y];
            for (int j = 0; j < Size_Y; j++)
            {
                gridArray[i][j] = MapGrid[i, j]?.ToString() ?? "_";
            }
        }

        return JsonSerializer.Serialize(gridArray);
    }
}
