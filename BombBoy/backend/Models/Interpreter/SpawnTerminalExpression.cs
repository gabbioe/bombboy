namespace BombBoy.Models.Interpreter;

/// <summary>
/// 'TerminalExpression' 3: Interprets entity/item spawning commands on the arena grid
/// (e.g., "spawn weapon", "spawn powerup", "spawn fusion", "spawn shield", "spawn bomb").
/// </summary>
public class SpawnTerminalExpression : AbstractConsoleExpression
{
    private readonly string _entityKind;

    public SpawnTerminalExpression(string entityKind)
    {
        _entityKind = (entityKind ?? "powerup").Trim().ToLowerInvariant();
    }

    public override void Interpret(ConsoleContext context)
    {
        if (_entityKind == "bomb")
        {
            var pos = context.Match.SpawnConsoleBomb(context.TargetPlayer);
            if (pos.HasValue)
                context.AppendOutput($"[Spawn] Dropped live bomb at ({pos.Value.x}, {pos.Value.y}) for {context.TargetPlayer.Name}.");
            else
                context.AppendOutput("[Warn] Could not find an open tile to spawn bomb.");
            return;
        }

        var itemPos = context.Match.SpawnConsoleItem(_entityKind);
        if (itemPos.HasValue)
        {
            context.AppendOutput($"[Spawn] Spawned '{_entityKind}' at ({itemPos.Value.x}, {itemPos.Value.y}).");
        }
        else
        {
            context.AppendOutput($"[Warn] Could not spawn '{_entityKind}' (no free floor tile or unknown item).");
        }
    }
}
