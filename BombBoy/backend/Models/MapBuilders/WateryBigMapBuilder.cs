using BombBoy.Models;
using BombBoy.Models.Tiles;
using System;
using System.Collections.Generic;
using System.Text;

namespace Bombboy.Models.MapBuilders
{
    public class WateryBigMapBuilder : IMapBuilder
    {
        private int _sizeX = 13;
        private int _sizeY = 13;

        private int _blockSpawnChance = 20;

        private int _WaterSpawnChance = 50;
        private MapTileFactory _factory;
        public IMapBuilder SetDimensions(int x, int y)
        {
            _sizeX = x;
            _sizeY = y;
            return this;
        }
        public IMapBuilder BuildBlockSpawnChance(int chance)
        {
            _blockSpawnChance = chance;
            return this;
        }

        public IMapBuilder BuildFactory(MapTileFactory factory)
        {
            _factory = factory;
            return this;
        }

        public IMapBuilder BuildWaterSpawnChance(int chance)
        {
            _WaterSpawnChance = chance;
            return this;
        }

        Map IMapBuilder.GetResult()
        {
            return new Map(_sizeX, _sizeY, _blockSpawnChance, _WaterSpawnChance, _factory);
        }
    }
}
