namespace BombBoy.Models;

public interface ICommand
{
    string Name { get; }
    void Execute();
    void Undo();
}