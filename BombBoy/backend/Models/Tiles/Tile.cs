using System;
using System.Collections.Generic;
using System.Reflection.Metadata.Ecma335;
using System.Text;

namespace BombBoy.Models.Tiles
{
    abstract public class Tile
    {
        protected string _jsonFormating;
        protected string _colour;
        public abstract bool CanWalk();

        //tarkime grazina ar mire
        public abstract bool DoDamage();

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
    }
}
