using BombBoy.Models.GridEntities;
using BombBoy.Models.Items;
using System.Text.Json;
using System.Text.Json.Nodes;

namespace BombBoy.Models.Visitor;

/// <summary>
/// Concrete Visitor that traverses Match elements (Map, Entity slots, Items, and Players)
/// to assemble the JSON representation for SignalR clients.
/// </summary>
public sealed class JsonMatchRenderVisitor : IMatchVisitor
{
    public JsonNode? MapLayer { get; private set; }
    public JsonObject EntityLayer { get; } = new();
    public JsonArray PlayerLayer { get; } = new();

    public void VisitMap(Map map)
    {
        var gridArray = new string[map.GetX()][];
        for (int i = 0; i < map.GetX(); i++)
        {
            gridArray[i] = new string[map.GetY()];
            for (int j = 0; j < map.GetY(); j++)
            {
                gridArray[i][j] = map.GetTile(i, j)?.ToString() ?? "_";
            }
        }
        MapLayer = JsonSerializer.SerializeToNode(gridArray);
    }

    public void VisitEntitySlot(int x, int y, Entity topEntity)
    {
        EntityLayer[$"{x},{y}"] = topEntity.ToString();
    }

    public void VisitItem(IItem item)
    {
        EntityLayer[$"{item.X},{item.Y}"] = item.GetEmoji();
    }

    public void VisitPlayer(Player player)
    {
        if (!player.IsAlive) return;

        var entry = new JsonObject
        {
            [$"{player.X},{player.Y}"] = player.GetPossitionAndDirectionJson()
        };
        PlayerLayer.Add(entry);
    }

    public string GetMapLayerJson() =>
        MapLayer?.ToJsonString() ?? "[]";

    public string BuildCombinedJson(bool allPlayersDead, int playerCount, bool writeIndented = false)
    {
        var combined = new JsonObject
        {
            ["MapLayer"] = MapLayer?.DeepClone() ?? new JsonArray(),
            ["EntityLayer"] = EntityLayer.DeepClone(),
            ["PlayerLayer"] = PlayerLayer.DeepClone(),
            ["AllPlayersDead"] = allPlayersDead,
            ["PlayerCount"] = playerCount
        };

        var options = new JsonSerializerOptions { WriteIndented = writeIndented };
        return combined.ToJsonString(options);
    }
}
