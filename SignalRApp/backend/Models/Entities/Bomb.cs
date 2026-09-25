using SignalRApp.Models.GridEntities;
using System;
using System.Collections.Generic;
using System.Text;

namespace SignalRApp.Models.GridEntities;

public  class Bomb : Entity
{

    public Bomb(int layer = 0, string type = "Bomb") : base(layer, type)
    {

    }

    public override bool CanWalk()
    {
        throw new NotImplementedException();
    }

    public override bool DoDamage()
    {
        throw new NotImplementedException();
    }

    public override string? ToString()
    {
        return base.ToString();
    }
}

