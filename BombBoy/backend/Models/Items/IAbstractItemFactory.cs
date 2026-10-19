namespace BombBoy.Models.Items
{
    public interface IAbstractItemFactory
    {
        Powerup CreatePowerup(int x, int y);
        Weapon CreateWeapon(int x, int y);
    }
}