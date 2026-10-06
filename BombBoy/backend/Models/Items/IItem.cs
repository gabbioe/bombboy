using BombBoy.Models; // for Player and Match
namespace BombBoy.Models.Items
{
    public interface IItem
    {
        int X { get; set; }
        int Y { get; set; }
        string GetEmoji(); // for frontend representation
        void Pickup(Player player);
        void Destroy();
    }
}