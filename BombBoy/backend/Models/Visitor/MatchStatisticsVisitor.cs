using Bombboy.Models.Tiles;
using BombBoy.Models.GridEntities;
using BombBoy.Models.Items;
using BombBoy.Models.Tiles;
using System.Collections.Generic;
using System.Text.Json.Nodes;

namespace BombBoy.Models.Visitor;

/// <summary>
/// Concrete Visitor 3: Traverses the Match object graph (Map, Entity slots, Items, and Players)
/// to aggregate live board statistics and extract item DTOs without coupling analytics to domain models.
/// </summary>
public sealed class MatchStatisticsVisitor : IMatchVisitor
{
    public int RemainingDestructibleBlocks { get; private set; }
    public int WaterTilesCount { get; private set; }
    public int ActiveBombsCount { get; private set; }
    public int ActiveBlastsCount { get; private set; }
    public int WeaponsOnGround { get; private set; }
    public int PowerupsOnGround { get; private set; }
    public int AlivePlayersCount { get; private set; }
    public int ShieldedPlayersCount { get; private set; }
    public int ArmedPlayersCount { get; private set; }
    public List<Match.ItemDto> ItemDtos { get; } = new();

    public void VisitMap(Map map)
    {
        for (int i = 0; i < map.GetX(); i++)
        {
            for (int j = 0; j < map.GetY(); j++)
            {
                var tile = map.GetTile(i, j);
                if (tile is DestructableTile)
                    RemainingDestructibleBlocks++;
                else if (tile is WaterTile)
                    WaterTilesCount++;
            }
        }
    }

    public void VisitEntitySlot(int x, int y, Entity topEntity)
    {
        if (topEntity is Bomb)
            ActiveBombsCount++;
        else if (topEntity is Blast)
            ActiveBlastsCount++;
    }

    public void VisitItem(IItem item)
    {
        if (item is Weapon)
            WeaponsOnGround++;
        else if (item is Powerup)
            PowerupsOnGround++;

        ItemDtos.Add(new Match.ItemDto(item.X, item.Y, item.GetEmoji()));
    }

    public void VisitPlayer(Player player)
    {
        if (!player.IsAlive)
            return;

        AlivePlayersCount++;
        if (player.HasShield)
            ShieldedPlayersCount++;
        if (player.CurrentWeapon != null)
            ArmedPlayersCount++;
    }

    public JsonObject ToJsonObject() => new()
    {
        ["RemainingDestructibleBlocks"] = RemainingDestructibleBlocks,
        ["WaterTilesCount"] = WaterTilesCount,
        ["ActiveBombsCount"] = ActiveBombsCount,
        ["ActiveBlastsCount"] = ActiveBlastsCount,
        ["WeaponsOnGround"] = WeaponsOnGround,
        ["PowerupsOnGround"] = PowerupsOnGround,
        ["AlivePlayersCount"] = AlivePlayersCount,
        ["ShieldedPlayersCount"] = ShieldedPlayersCount,
        ["ArmedPlayersCount"] = ArmedPlayersCount
    };
}
