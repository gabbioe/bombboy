using System;
using System.Collections.Generic;
using System.Text;

namespace SignalRApp.Models.GridEntities;

internal class Air : Cell
{
    public Air(string type = "Air") : base(type)
    {

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
        return "_";
    }
}
