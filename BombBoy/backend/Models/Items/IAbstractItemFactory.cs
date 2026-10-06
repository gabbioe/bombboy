namespace BombBoy.Models.Items
{
    public interface IAbstractItemFactory
    {
        Powerup CreatePowerup(int x, int y);
        Weapon CreateWeapon(int x, int y);
    }

    public interface OneUseFactory : IAbstractItemFactory
    {
        // specific one-use items can be defined here if needed
    }

    public interface MultipleUseFactory : IAbstractItemFactory
    {
        // specific multiple-use items can be defined here if needed
    }
}