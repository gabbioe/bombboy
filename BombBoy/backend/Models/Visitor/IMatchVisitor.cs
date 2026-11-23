using BombBoy.Models.GridEntities;
using BombBoy.Models.Items;

namespace BombBoy.Models.Visitor;

public interface IMatchVisitor
{
    void VisitMap(Map map);
    void VisitEntitySlot(int x, int y, Entity topEntity);
    void VisitItem(IItem item);
    void VisitPlayer(Player player);
}
