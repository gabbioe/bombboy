namespace BombBoy.Models.Visitor;

public interface IMatchVisitable
{
    void Accept(IMatchVisitor visitor);
}
