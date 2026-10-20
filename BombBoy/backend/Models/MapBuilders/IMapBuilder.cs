using BombBoy.Models;
using BombBoy.Models.Tiles;
using Microsoft.AspNetCore.DataProtection.KeyManagement.Internal;
using System;
using System.Collections.Generic;
using System.Text;

namespace Bombboy.Models.MapBuilders
{
    public interface IMapBuilder
    {
        public abstract IMapBuilder SetDimensions(int x, int y);
        public abstract IMapBuilder BuildBlockSpawnChance(int chance);
        public abstract IMapBuilder BuildWaterSpawnChance(int chance);
        public abstract IMapBuilder BuildHavePillars(bool havePillars);
        public abstract IMapBuilder BuildFactory(MapTileFactory factory);

        public abstract Map GetResult();

            

    }
}
