namespace BombBoy.Models.GridEntities;

using BombBoy.Models.Strategies;

public class Bomb : Entity
{
    public int X { get; set; }
    public int Y { get; set; }
    public int Range { get; set; }
    public float Timer { get; set; } = 3.0f; // 3 seconds until detonation
    public string OwnerId { get; set; }

    // Strategy Pattern Context
    public IExplosionStrategy Strategy { get; set; }

    public Bomb(int x, int y, int range, IExplosionStrategy strategy, string ownerId) 
        : base(layer: 1, type: "💣")
    {
        X = x;
        Y = y;
        Range = range;
        Strategy = strategy ?? new ClassicCrossExplosionStrategy();
        OwnerId = ownerId;
    }

    public override bool CanWalk() => false; // Blocks player walking

    // Chain reaction: detonates immediately when hit by another blast
    public override bool DoDamage()
    {
        Timer = 0f;
        return true;
    }

    public bool Tick(float deltaTime)
    {
        Timer -= deltaTime;
        return Timer <= 0;
    }

    public void Detonate(Map map)
    {
        // Delegate explosion to the strategy
        Strategy.Explode(map, X, Y, Range);
    }
}