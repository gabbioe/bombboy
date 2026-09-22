using System;
using System.Collections.Generic;
using System.Numerics;
using System.Text;
using SignalRApp.Models;

namespace SignalRApp.Models.GridEntities;


public class PlayerCell : Cell
{
    //Galvoju turet dar Vardo string variable kur laikytu pvz p1 arba pRed, ir butu galima naudot sitai persiunciant info klientui

    public string Id { get; set; }

    //net nzn ara butinas bet kolkas yra tarkim
    private Player player {  get; set; }

    public PlayerCell(Player playa, string id = "ID_Of_Player", string type = "Player") : base(type)
    {
        player = playa;
        Id = id; // !!! pakeisti i actual id, neaisku kur ka des tai dabar tik paruosiu
    }

    public override bool CanWalk()
    {
        return false;
    }

    public override Cell? DoDamage()
    {
        player.isAlive = false;
        return new Air();
    }

    public override string? ToString()
    {
        return Id;
    }
}

