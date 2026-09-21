using System;
using System.Collections.Generic;
using System.Text;

namespace SignalRApp.Models.GridEntities;

public class Blast : Cell
{
    public Blast (string type = "Blast") : base(type)
    {

    }

    public override bool CanWalk()
    {
        throw new NotImplementedException();
    }

    public override Cell? DoDamage()
    {
        throw new NotImplementedException();
    }

    public override string? ToString()
    {
        return base.ToString();
    }
}

