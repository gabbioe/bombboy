namespace SignalRApp.Models;

public class Player
{
    public string Id{get;set;} = string.Empty;
    public string Name{get;set;} = "Player";
    public float X{get;set;} = 0;
    public float Y{get;set;} = 0;
    public bool isAlive{get;set;} = true;
    public int BombCount{get;set;} = 1;
    public int Explosion{get;set;} = 1;
}