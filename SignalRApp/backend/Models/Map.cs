using Microsoft.AspNetCore.Authorization;
using SignalRApp.Models;
using SignalRApp.Models.GridEntities;
using System;
using System.ComponentModel.DataAnnotations;

namespace SignalRApp.Models;



public class Map
{
    private int Size_X {  get; set; }

    private int Size_Y { get; set; }


    private Cell[,] MapGrid;
    private Dictionary<string, (int x, int y)> PlayerLocations = new();

    private int PlayerCount = 0;
    private Stack<int[]> AvailableStartingPositions = new();

    /// <summary>
    /// Generates the map
    /// </summary>
    /// <param name="size_X"></param>
    /// <param name="size_Y"></param>
    /// <param name="BlockSpawnChance"> kokios tikimybes kad bus sunaikinamas blokas is 10 (pvz 1 is 10 tai 10% kad laisva vieta bus blokas)</param>
    /// <exception cref="ArgumentException"></exception>
    public Map([Range(5, int.MaxValue)] int size_X, [Range(5, int.MaxValue)] int size_Y, int BlockSpawnChance )
    {

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

        AvailableStartingPositions.Push([Size_X - 1, Size_Y - 1]);
        AvailableStartingPositions.Push([1, Size_Y - 1]);
        AvailableStartingPositions.Push([Size_X - 2, 1]);
        AvailableStartingPositions.Push([1, 1]);

        generateMapGood(BlockSpawnChance);
        //GenerateMapSimple();
    }

    private void generateMapGood(int BlockSpawnChance)
    {
        Random rand = new Random();
        MapGrid = new Cell[Size_X, Size_Y];
        for (int i = 0; i < Size_X; i++)
        {
            for (int j = 0; j < Size_Y; j++)
            {
                //borderiai
                if (i == 0 || i == Size_X - 1 || j == 0 || j == Size_Y - 1)
                    MapGrid[i, j] = new Block(false);
                //kas antra bloka pazymi kad butu 
                if (i % 2 == 0 && j % 2 == 0)
                    MapGrid[i, j] = new Block(false);
                //Tikrina ar nera kampuose kur playeriai spawninasi 
                if (MapGrid[i, j] == null)
                {
                    if ((i > 2 || j > 2) && (i < Size_X - 3 || j > 2) && (i > 2 || j < Size_Y - 3) && (i < Size_X - 3 || j < Size_Y - 3))
                    {
                        if (rand.Next(0, 10) < BlockSpawnChance)
                            MapGrid[i, j] = new Block(true);
                        else
                            MapGrid[i, j] = new Air("Air");
                    }
                    else
                        MapGrid[i, j] = new Air("Air");
                }
            }
        }
    }


    public int[] AddPlayer(Player newPlayer)
    {
        int[] playerLocation = AvailableStartingPositions.Pop();
        if (PlayerCount < 3)
        {
            PlayerCount++;
            PlayerLocations[newPlayer.Id] = (playerLocation[0], playerLocation[0]);
            return playerLocation;
        }

        throw new ArgumentException($"Game only supports up too 4 players");
    }

    //tik borderiai
    private void GenerateMapSimple()
    {
        MapGrid = new Cell[Size_X, Size_Y];
        for (int i = 0; i < Size_X; i++)
        {
            for (int j = 0; j < Size_Y; j++)
            {
                //borderiai
                if (i == 0 || i == Size_X - 1 || j == 0 || j == Size_Y - 1)
                    MapGrid[i, j] = new Block(false);
                else
                    MapGrid[i, j] = new Air("Air");
                
            }
        }
    }


    //proly nebereiks (
    public bool CheckIfCanMove(int x, int y)
    {
        return MapGrid[x,y].CanWalk();
    }

    private bool checkIfCanMove(int x, int y)
    {
        return MapGrid[x, y].CanWalk();
    }

    /// <summary>
    /// moves the player on the map
    /// </summary>
    /// <param name="dx"> change in possition</param>
    /// <param name="dy"> change in possition</param>
    /// <param name="PlayerId"></param>
    /// <returns>True if player did move, False if player did not move</returns>
    public bool Move(int dx, int dy, string PlayerId)
    {
        if(PlayerLocations.TryGetValue(PlayerId, out var player) )
        {
            if (Math.Abs(dx) + Math.Abs(dy) < 2){
                int x = player.x;
                int y = player.y;
                if (checkIfCanMove(x + dx, y + dx))
                {
                    MapGrid[x + dx, y + dx] = MapGrid[x, y];
                    MapGrid[x, y] = new Air();
                }
                return false;
            }
            throw new ArgumentException($"Player can only move in one direction at once, either dx({dx}) or dy({dy}) MUST be zero");
        }
        throw new ArgumentException($"Player with id [{PlayerId}] Doesn't exist");
        return false;
    }

    //external damage, gal nebereiks Jei game logika mapse :D
    public void DoDamange(int x, int y)
    {
        var result = MapGrid[x + 1, y + 1].DoDamage();
        if (result != null)
            MapGrid[x + 1, y + 1] = result;
    }

    private void doDamange(int x, int y)
    {
        var result = MapGrid[x, y].DoDamage();
        if (result != null)
            MapGrid[x, y] = result;
    }


    //TODO (gal) padaryti kad gerai printintui klientui
    public string ReturnMap()
    {
        string result = string.Empty;

        for (int i = 0; i < Size_X; i++)
        {
            for (int j = 0; j < Size_Y; j++)
            {
                //Console.WriteLine($"i == {i} j == {j} Type == {MapGrid[i, j].Type}");
                result += MapGrid[i, j];
            }
            result += "\n";
        }
        return result;
    }


}
