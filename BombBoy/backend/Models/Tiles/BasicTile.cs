using System;
using System.Collections.Generic;
using System.Reflection.Metadata.Ecma335;
using System.Text;

namespace BombBoy.Models.Tiles
{
    public class BasicTile
    {
        protected string _jsonFormating;
        protected string _colour;

        protected bool CanWalk;

        public string Type;
        public BasicTile(string col, string formating, string type, bool canWalk)
        {
            _colour = col;
            _jsonFormating = formating;
            Type = type;
            CanWalk = canWalk;
        }

        public virtual bool CanWalkTo()
        {
            return CanWalk;
        }

        //tarkime grazina ar mire
        public virtual bool DoDamage()
        {
            return false;
        }

        //returns how to colour the block in the Client 
        //(kolkas paprastas, veliau gal pilnai .css stuff ar image jei noresim
        public virtual string Colour()
        {
            return _colour;
        }

        //!!!!!!!!!!! Sita butinai reikia turet implementuota, jis naudojamas kad perduot mapo informacija klientui
        public virtual string? ToString()
        {
            return _jsonFormating;
        }

        public virtual BasicTile Clone()
        {
            return (BasicTile)this.MemberwiseClone();
        }
    }
}
