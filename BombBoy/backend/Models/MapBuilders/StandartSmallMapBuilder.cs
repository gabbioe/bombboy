using BombBoy.Models;
using BombBoy.Models.Tiles;

namespace Bombboy.Models.MapBuilders
{
    public class StandartSmallMapBuilder : IMapBuilder
    {
        private int _sizeX = 7;
        private int _sizeY = 7;
        private int _blockSpawnChance = 50;
        private int _waterSpawnChance = 10;
        private bool _havePillars = true;
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
            _waterSpawnChance = chance;
            return this;
        }

        public IMapBuilder BuildHavePillars(bool havePillars)
        {
            _havePillars = havePillars;
            return this;
        }

        public Map GetResult()
        {
            return new Map(_sizeX, _sizeY, _blockSpawnChance, _waterSpawnChance, _factory, _havePillars);
        }
    }
}
