using System;
using System.Collections.Generic;
using System.Text;

namespace SignalRApp.Models.GridEntities;

public class Player : Cell
{
    //Galvoju turet dar Vardo string variable kur laikytu pvz p1 arba pRed, ir butu galima naudot sitai persiunciant info klientui

    public Player(string type = "Player") : base(type)
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

