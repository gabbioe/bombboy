using System;
using System.Collections.Generic;
using System.Text;

namespace BombBoy.Models.Tiles
{
    public class DestructableTile : BasicTile
    {
        public int Health;
        public DestructableTile(string col, string formating, string type, bool canWalk, int health = 1) :base(col, formating, type, canWalk)
        {
            //_colour = col;
            //_jsonFormating = "B";
            Health = health;
            //Type = "DestructableTile";
        }
        public override bool DoDamage()
        {
            Health--;
            if (Health <= 0)
                return true;
            return false;
        }
    }
}
