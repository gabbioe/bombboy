using System;
using System.Collections.Generic;
using System.Text;

namespace SignalRApp.Models.GridEntities;

public abstract class Cell
{
    //pavadinimui, dar niekam nenaudojom ;/

    public string Type { get; set; }

    public Cell (string type = "Cell")
    {
        Type = type;
    }

    public abstract bool CanWalk();

    //tarkime gali grazinti kita objekta(ora, itema, bbz kazka kitka)
    public abstract Cell? DoDamage();

    //!!!!!!!!!!! Sita butinai reikia turet implementuota, jis naudojamas kad perduot mapo informacija klientui
    //public abstract override string? ToString()
    
        
}

