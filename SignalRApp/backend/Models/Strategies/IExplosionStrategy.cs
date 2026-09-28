namespace SignalRApp.Models.Strategies;

public interface IExplosionStrategy
{
    void Explode(Map map, int originX, int originY, int range);
}
