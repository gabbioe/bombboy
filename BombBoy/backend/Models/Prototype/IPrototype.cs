namespace BombBoy.Models;

public interface IPrototype<T>
{
    T ShallowCopy();
    T DeepCopy();
}