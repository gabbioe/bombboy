using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;
using BombBoy.Models;
using BombBoy.Models.Tiles;
using System;
using System.ComponentModel.DataAnnotations;
using System.Text.Json;


namespace BombBoy.Models;

public class Map : IPrototype<Map>
{
    private int Size_X {  get; set; }

    private int Size_Y { get; set; }

    private BasicTile[,] MapTileGrid;

    public string? CurrentBlastOwnerId { get; set; }
    public event Action<int, int, int>? OnBlastSpawned;
    public event Action<int, int, int, string?>? OnBlastSpawnedWithOwner;
    public event Action<int, int>? OnBlockDestroyed;

    public MapTileFactory Factory;


    /// <summary>
    /// Generates the map
    /// </summary>
    /// <param name="size_X"></param>
    /// <param name="size_Y"></param>
    /// <param name="BlockSpawnChance"> kokios tikimybes kad bus sunaikinamas blokas is 10 (pvz 1 is 10 tai 10% kad laisva vieta bus blokas)</param>
    /// <exception cref="ArgumentException"></exception>
    public Map(int size_X, int size_Y, int BlockSpawnChance, int WaterSpawnChance, MapTileFactory factory, bool HavePillars)
    {
        Factory = factory;
        if (HavePillars)
        {
            if (size_X < 5 || size_Y < 5)
            {
                throw new ArgumentException($"Both Size X ({size_X}) and Size Y ({size_Y}) must be at least 5.");
            }

            if (size_X % 2 == 0 || size_Y % 2 == 0)
            {
                throw new ArgumentException($"Both Size X ({size_X}) and Size Y ({size_Y}) must NOT be even.");
            }
        }
        

        //del borderiu +2 
        Size_X = size_X +2;
        Size_Y = size_Y +2;
        
        //stackas jei apdorosim rejoininima, jei ne galime tsg masyva turet
        
        generateMapGood(BlockSpawnChance, WaterSpawnChance, HavePillars);
        //GenerateMapSimple();
    }

    private void generateMapGood(int BlockSpawnChance, int WaterSpawnChance, bool HavePillars)
    {
        //Console.WriteLine("Generuoju mapa!");

        Random rand = new Random();
        MapTileGrid = new BasicTile[Size_X, Size_Y];
        for (int i = 0; i < Size_X; i++)
        {
            for (int j = 0; j < Size_Y; j++)
            {
                //borderiai
                if (i == 0 || i == Size_X - 1 || j == 0 || j == Size_Y - 1)
                    MapTileGrid[i, j] = Factory.CreateIndestructableTile();
                //kas antra bloka pazymi kad butu 
                if (HavePillars && i % 2 == 0 && j % 2 == 0)
                    MapTileGrid[i, j] = Factory.CreateIndestructableTile();
                //Tikrina ar nera kampuose kur playeriai spawninasi 
                if (MapTileGrid[i, j] == null)
                {
                    if ((i > 2 || j > 2) && (i < Size_X - 3 || j > 2) && (i > 2 || j < Size_Y - 3) && (i < Size_X - 3 || j < Size_Y - 3))
                    {
                        if (rand.Next(0, 100) < BlockSpawnChance)
                            MapTileGrid[i, j] = Factory.CreateDestructableTile();
                        else if (rand.Next(0, 100) < WaterSpawnChance)
                            MapTileGrid[i, j] = Factory.CreateWaterTile();
                        else
                            MapTileGrid[i, j] = Factory.CreateFloorTile();
                    }
                    else
                        MapTileGrid[i, j] = Factory.CreateFloorTile();
                }
            }
        }
    }
    public BasicTile? GetTile(int x, int y)
    {
        if (!IsValidCoord(x, y)) return null;
        return MapTileGrid[x, y];
    }
    public void DestroyTile(int x, int y)
    {
        if (!IsValidCoord(x, y)) return;
        if (MapTileGrid[x, y] is DestructableTile)
        {
            MapTileGrid[x, y] = Factory.CreateFloorTile();
            OnBlockDestroyed?.Invoke(x, y); // trigger item spawn
        }
    }

    public bool CheckIfCanMove(int x, int y)
    {
        return MapTileGrid[x, y].CanWalkTo();
    }

    public bool IsValidCoord(int x, int y)
    {
        return x >= 0 && x < Size_X && y >= 0 && y < Size_Y;
    }

    public void SpawnBlast(int x, int y, int durationTicks = 10)
    {
        // 1. Boundary check
        if (!IsValidCoord(x, y)) return;
        if (MapTileGrid[x, y] == null) return;
        // 2. Do not spawn blast on indestructible borders or pillars
        if (MapTileGrid[x, y].Type == "IndestructableTile") return;
        // 3. If it's a destructible block, damage it and turn it into floor
        if (MapTileGrid[x, y].DoDamage())
        {
            MapTileGrid[x, y] = Factory.CreateFloorTile();
            // notify Match to potentially spawn an item
            OnBlockDestroyed?.Invoke(x, y);
        }
        // 4. Notify Match to add the Blast entity above this tile
        OnBlastSpawned?.Invoke(x, y, durationTicks);
        OnBlastSpawnedWithOwner?.Invoke(x, y, durationTicks, CurrentBlastOwnerId);
    }


    //TODO (gal) padaryti kad gerai printintui klientui
    public string ReturnMapToConsole()
    {
        string result = string.Empty;

        for (int i = 0; i < Size_X; i++)
        {
            for (int j = 0; j < Size_Y; j++)
            {
               //Console.WriteLine($"i == {i} j == {j} Type == {MapGrid[i, j].Type}");
               result += MapTileGrid[i, j].ToString();
            }
            result += "\n";
        }
        //Console.WriteLine(result);
        return result;
    }


    public string ReturnMapToJSon()
    {
        // Create a jagged array matching your grid dimensions
        var gridArray = new string[Size_X][];

        for (int i = 0; i < Size_X; i++)
        {
            gridArray[i] = new string[Size_Y];
            for (int j = 0; j < Size_Y; j++)
            {
                gridArray[i][j] = MapTileGrid[i, j].ToString();
            }
        }
        // Serialize to a JSON string
        return JsonSerializer.Serialize(gridArray);
    }

    public int GetX()
    {
        return Size_X;
    }
    public int GetY()
    {
        return Size_X;
    }

    public Map ShallowCopy()
    {
        return (Map)this.MemberwiseClone();
    }
    public Map DeepCopy()
    {
        Map clone = (Map)this.MemberwiseClone();
        clone.MapTileGrid = new BasicTile[Size_X, Size_Y];

        for(int i = 0; i < Size_X; i++)
        {
            for(int j = 0; j < Size_Y; j++)
            {
                clone.MapTileGrid[i, j] = this.MapTileGrid[i, j].Clone();
            }
        }
        clone.OnBlastSpawned = null;
        clone.OnBlastSpawnedWithOwner = null;
        clone.OnBlockDestroyed = null;
        return clone;
    }
}
