using SignalRApp.Models.Strategies;

namespace SignalRApp.Models.GridEntities;

public class Bomb : Cell
{
    private IExplosionStrategy _strategy;

    public int Range { get; set; } = 2;
    public int FuseTicks { get; set; } = 40; // ~2 seconds at 50ms/tick
    public int X { get; set; }
    public int Y { get; set; }
    public string OwnerId { get; set; } = string.Empty;
    public bool HasExploded { get; set; } = false;

    public Bomb(IExplosionStrategy strategy, int x, int y, int range = 2, int fuseTicks = 40, string ownerId = "") : base("Bomb")
    {
        _strategy = strategy;
        X = x;
        Y = y;
        Range = range;
        FuseTicks = fuseTicks;
        OwnerId = ownerId;
    }

    public Bomb(string type = "Bomb") : this(new ClassicCrossExplosionStrategy(), 0, 0)
    {
        Type = type;
    }

    public void SetExplosionStrategy(IExplosionStrategy strategy)
    {
        _strategy = strategy;
    }

    public IExplosionStrategy GetExplosionStrategy() => _strategy;

    public void Explode(Map map)
    {
        if (HasExploded) return;
        HasExploded = true;
        _strategy.Explode(map, X, Y, Range);
    }

    public override bool CanWalk()
    {
        return false;
    }

    public override Cell? DoDamage()
    {
        // When damaged by an explosion, bomb detonates/clears
        return new Air();
    }

    public override string? ToString()
    {
        return "O";
    }
}
