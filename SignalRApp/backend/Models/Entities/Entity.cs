using System;
using System.Collections.Generic;
using System.Text;

namespace SignalRApp.Models.GridEntities;

public abstract class Entity : IComparable<Entity>
{
    //pavadinimui, dar niekam nenaudojom ;/
    public string JsonFormating { get; set; }

    public Entity(int layer = 0, string type = "C")
    {
        DrawingLayer = layer;
        JsonFormating = type;
    }

    //piesimo lygis, pies entity su maziausiu value, pvz tarp dvieju entities su drawing layer = 0 ir 1 ims ta kuris turi drawling layer 0
    private int DrawingLayer {get;set;}    

    //Probably reikes naujo/atskiro functiono kur butu kokia nors uzlipimo / pickup funckija
    public abstract bool CanWalk();

    //tarkime gali grazinti kita objekta(ora, itema, bbz kazka kitka)
    public abstract bool DoDamage();

    public override string? ToString()
    {
        return JsonFormating;
    }

    public int CompareTo(Entity? other)
    {
        return this.DrawingLayer.CompareTo(other.DrawingLayer);
    }
}

