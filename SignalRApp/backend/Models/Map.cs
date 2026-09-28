using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;
using SignalRApp.Models;
using SignalRApp.Models.GridEntities;
using SignalRApp.Models.TileFactories;
using SignalRApp.Models.Tiles;
using System;
using System.ComponentModel.DataAnnotations;
using System.Text.Json;


namespace SignalRApp.Models;

public class Map
{
    private int Size_X {  get; set; }

    private int Size_Y { get; set; }

    private Tile[,] MapTileGrid;

    private IMapTileFactory tileFactory;

    

    /// <summary>
    /// Generates the map
    /// </summary>
    /// <param name="size_X"></param>
    /// <param name="size_Y"></param>
    /// <param name="BlockSpawnChance"> kokios tikimybes kad bus sunaikinamas blokas is 10 (pvz 1 is 10 tai 10% kad laisva vieta bus blokas)</param>
    /// <exception cref="ArgumentException"></exception>
    public Map(int size_X, int size_Y, int BlockSpawnChance, IMapTileFactory factory)
    {
        tileFactory = factory;

        if (size_X < 5 || size_Y < 5)
        {
            throw new ArgumentException($"Both Size X ({size_X}) and Size Y ({size_Y}) must be at least 5.");
        }

        if (size_X % 2 == 0 || size_Y % 2 == 0)
        {
            throw new ArgumentException($"Both Size X ({size_X}) and Size Y ({size_Y}) must NOT be even.");
        }

        //del borderiu +2 
        Size_X = size_X +2;
        Size_Y = size_Y +2;
        
        //stackas jei apdorosim rejoininima, jei ne galime tsg masyva turet
        
        generateMapGood(BlockSpawnChance);
        //GenerateMapSimple();
    }

    private void generateMapGood(int BlockSpawnChance)
    {
        Console.WriteLine("Generuoju mapa!");

        Random rand = new Random();
        MapTileGrid = new Tile[Size_X, Size_Y];
        for (int i = 0; i < Size_X; i++)
        {
            for (int j = 0; j < Size_Y; j++)
            {
                //borderiai
                if (i == 0 || i == Size_X - 1 || j == 0 || j == Size_Y - 1)
                    MapTileGrid[i, j] = tileFactory.CreateIndestructableTile();
                //kas antra bloka pazymi kad butu 
                if (i % 2 == 0 && j % 2 == 0)
                    MapTileGrid[i, j] = tileFactory.CreateIndestructableTile();
                //Tikrina ar nera kampuose kur playeriai spawninasi 
                if (MapTileGrid[i, j] == null)
                {
                    if ((i > 2 || j > 2) && (i < Size_X - 3 || j > 2) && (i > 2 || j < Size_Y - 3) && (i < Size_X - 3 || j < Size_Y - 3))
                    {
                        if (rand.Next(0, 100) < BlockSpawnChance)
                            MapTileGrid[i, j] = tileFactory.CreateDestructableTile();
                        else
                            MapTileGrid[i, j] = tileFactory.CreateFloorTile();
                    }
                    else
                        MapTileGrid[i, j] = tileFactory.CreateFloorTile();
                }
            }
        }
    }

    
    public bool CheckIfCanMove(int x, int y)
    {
        return MapTileGrid[x, y].CanWalk();
    }
    //extermal
    public void DoDamange(int x, int y)
    {
        doDamange(x + 1, y + 1);
    }

    //internals
    private void doDamange(int x, int y)
    {
        bool result = MapTileGrid[x, y].DoDamage();
        if (result)
            MapTileGrid[x, y] = tileFactory.CreateFloorTile();
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
               result += MapTileGrid[i, j];
            }
            result += "\n";
        }
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


}
