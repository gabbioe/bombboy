using SignalRApp.Models.GridEntities;
using System;
using System.Collections.Generic;
using System.Text;

namespace SignalRApp.Models.GridEntities;

public  class Bomb : Cell
{

    public Bomb(string type = "Bomb") : base(type)
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

