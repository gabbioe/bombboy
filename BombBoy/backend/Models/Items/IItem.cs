using BombBoy.Models; // for Player and Match
using BombBoy.Models.Visitor;

namespace BombBoy.Models.Items
{
    public interface IItem : IMatchVisitable
    {
        int X { get; set; }
        int Y { get; set; }
        string GetEmoji(); // for frontend representation
        void Pickup(Player player);
        void Destroy();
    }
}