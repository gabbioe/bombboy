using BombBoy.Models.GridEntities;
using BombBoy.Models.Items;
using System.Text;

namespace BombBoy.Models.Visitor;

/// <summary>
/// Concrete Visitor that traverses the Map to produce a console/ASCII text rendering.
/// </summary>
public sealed class ConsoleMapRenderVisitor : IMatchVisitor
{
    private readonly StringBuilder _builder = new();

    public string Result => _builder.ToString();

    public void VisitMap(Map map)
    {
        _builder.Clear();
        for (int i = 0; i < map.GetX(); i++)
        {
            for (int j = 0; j < map.GetY(); j++)
            {
                _builder.Append(map.GetTile(i, j)?.ToString() ?? "_");
            }
            _builder.Append('\n');
        }
    }

    public void VisitEntitySlot(int x, int y, Entity topEntity) { }
    public void VisitItem(IItem item) { }
    public void VisitPlayer(Player player) { }
}
