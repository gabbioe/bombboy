using BombBoy.Models;
using BombBoy.Models.Tiles;

namespace Bombboy.Models.MapBuilders
{
    public interface IMapBuilder
    {
        IMapBuilder SetDimensions(int x, int y);
        IMapBuilder BuildBlockSpawnChance(int chance);
        IMapBuilder BuildWaterSpawnChance(int chance);
        IMapBuilder BuildHavePillars(bool havePillars);
        IMapBuilder BuildFactory(MapTileFactory factory);
        Map GetResult();
    }
}
