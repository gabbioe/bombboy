using System.Text.Json;
using System.Text.Json.Nodes;

namespace SignalRApp.Models;

public class Player
{
    public string Id{get;set;} = string.Empty;
    public string Name{get;set;} = "Player";
    public int X{get;set;} = 0;
    public int Y {get;set;} = 0;

    public int DeathCount { get; set; } = 0;
    public int KillCount { get; set; } = 0;

    public string Colour { get; set; }

    public string Direction = "Down";
    public bool IsAlive{get;set;} = true;
    public int BombCount{get;set;} = 1;
    public int Explosion{get;set;} = 1;

    //reikalingas tarp match, galvoju sita padaryti matcho player klase, game manager atskira turetu turet
    public int StartingLocation { get;set;}

    public JsonObject GetPossitionAndDirectionJson()
    {
        
        return new JsonObject
        {
            ["Id"] = Id,
            ["Direction"] = Direction
        }; ;
    }
}