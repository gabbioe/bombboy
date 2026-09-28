using System;
using System.Collections.Generic;
using System.Text;

namespace SignalRApp.Models.GridEntities;

public abstract class Entity : IComparable<Entity>
{
    //pavadinimui, dar niekam nenaudojom ;/
    protected string _jsonFormating { get; set; }

    public Entity(int layer = 0, string type = "C")
    {
        _drawingLayer = layer;
        _jsonFormating = type;
    }

    //piesimo lygis, pies entity su maziausiu value, pvz tarp dvieju entities su drawing layer = 0 ir 1 ims ta kuris turi drawling layer 0
    private int _drawingLayer {get;set;}    

    //Probably reikes naujo/atskiro functiono kur butu kokia nors uzlipimo / pickup funckija
    public abstract bool CanWalk();

    //tarkime gali grazinti kita objekta(ora, itema, bbz kazka kitka)
    public abstract bool DoDamage();

    public override string? ToString()
    {
        return _jsonFormating;
    }

    public int CompareTo(Entity? other)
    {
        return this._drawingLayer.CompareTo(other._drawingLayer);
    }
}

