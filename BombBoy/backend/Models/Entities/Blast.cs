namespace BombBoy.Models.GridEntities;

public class Blast : Entity
{
    public float Duration { get; set; } = 0.5f; 
    public int X { get; set; }
    public int Y { get; set; }
    public string? OwnerId { get; set; }

    public Blast(int x, int y, float duration = 0.5f, string? ownerId = null) : base(layer: 0, type: "💥")
    {
        X = x;
        Y = y;
        Duration = duration;
        OwnerId = ownerId;
    }

    public override bool CanWalk() => true;

    public override bool DoDamage()
    {
        // Blasts do not take damage from other blasts
        return false;
    }

    public bool Tick(float deltaTime)
    {
        Duration -= deltaTime;
        return Duration <= 0; // True if blast has expired
    }
}