namespace BombBoy.Models;


public class CheatCode_PassWalls : ICommand
{
    private readonly Player _player;                                                                          
    public string Name => "noclip";

    public CheatCode_PassWalls(Player player)
    {
        _player = player;
    }
    public void Execute()
    {
       _player.CanPassThroughWalls = true;
        
    }

    public void Undo()
    {
        _player.CanPassThroughWalls = false;
    }
}

public class CheatCode_InfiniteBombs : ICommand
{
    private readonly Player _player;                                                                             
    private int _previousBombCount;
    public string Name => "infinitebombs";

    public CheatCode_InfiniteBombs(Player player)
    {
        _player = player;
    }
    public void Execute()
    {
        _previousBombCount = _player.BombCount;
        _player.BombCount = 99;
    }

    public void Undo()
    {
        _player.BombCount = _previousBombCount;
    }
}