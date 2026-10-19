using System;
using System.Collections.Generic;
using System.Text;
using BombBoy.Models;
using BombBoy.Models.Tiles;


namespace Bombboy.Models.MapBuilders
{
    public class StandartSmallMapBuilder : IMapBuilder
    {
        private int _sizeX = 7;
        private int _sizeY = 7;

        private int _blockSpawnChance = 50;

        private int _WaterSpawnChance = 10;
        private MapTileFactory _factory = new MapTileFactory();
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
