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

    public Map([Range(5, int.MaxValue)] int size_X, [Range(5, int.MaxValue)] int size_Y)
    {

        if (size_X < 5 || size_Y < 5)
        {
            throw new ArgumentException($"Both Size X ({size_X}) and Size Y ({size_Y}) must be at least 5.");
        }

        if (size_X % 2 == 0 || size_Y % 2 == 0)
        {
            throw new ArgumentException($"Both Size X ({size_X}) and Size Y ({size_Y}) must NOT be even.");
        }
        



        Random rand = new Random();
        // kokios tikimybes kad bus sunaikinamas blokas is 10 (pvz 1 is 10 tai 10% kad laisva vieta bus blokas)
        int BlockSpawnChance = 10;

        //del borderiu +2 
        Size_X = size_X +2;
        Size_Y = size_Y +2;
        MapGrid = new Cell[Size_X , Size_Y];
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

    public bool CheckForBlock(int x, int y)
    {
        return MapGrid[x+1,y+1].CanWalk();
    }

    public void DoDamange(int x, int y)
    {
        if (MapGrid[x + 1, y + 1].DoDamage() != null)
            MapGrid[x + 1, y + 1] = MapGrid[x + 1, y + 1].DoDamage();
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
