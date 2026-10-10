using System;
using System.Collections.Generic;
using System.Reflection.Metadata.Ecma335;
using System.Text;

namespace BombBoy.Models.Tiles
{
    public class Tile
    {
        protected string _jsonFormating;
        protected string _colour;

        public string Type;
        public Tile(string col)
        {
            _colour = col;
            _jsonFormating = "_";
            Type = "Tile";
        }

        public virtual bool CanWalk()
        {
            return true;
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
    }
}
