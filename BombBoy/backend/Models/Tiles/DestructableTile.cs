using System;
using System.Collections.Generic;
using System.Text;

namespace BombBoy.Models.Tiles
{
    public class DestructableTile : Tile
    {
        public int Health;
        public DestructableTile(string col, int health = 1) :base(col)
        {
            _colour = col;
            _jsonFormating = "B";
            Health = health;
            Type = "DestructableTile";
        }
        public override bool CanWalk()
        {
            return false;
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
