namespace SignalRApp.Models.GridEntities;



public class Block : Cell
{
    

    //public int Position_X {  get; set; }
    //public int Position_Y { get; set; }

    public bool Is_Breakable { get; set; }

    public Block(bool canBreak, string type = "Block") : base(type)
    {
        Is_Breakable = canBreak;
    }

    public override bool CanWalk()
    {
        return false;
    }

    public override Cell? DoDamage()
    {
        if (Is_Breakable)
            return new Air();
        return null;
    }

    //naudojamas mapo printimui
    public override string? ToString()
    {
        if (Is_Breakable)
            return "B";
        return "X";
    }
}
