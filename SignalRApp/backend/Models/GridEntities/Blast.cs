namespace SignalRApp.Models.GridEntities;

public class Blast : Cell
{
    public int DurationTicks { get; set; }

    public Blast(int durationTicks = 15, string type = "Blast") : base(type)
    {
        DurationTicks = durationTicks;
    }

    public override bool CanWalk()
    {
        return true;
    }

    public override Cell? DoDamage()
    {
        return null;
    }

    public override string? ToString()
    {
        return "*";
    }
}
