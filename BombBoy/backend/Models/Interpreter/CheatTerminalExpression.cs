namespace BombBoy.Models.Interpreter;

/// <summary>
/// 'TerminalExpression' 1: Interprets cheat activation and undo tokens
/// (e.g., "noclip", "infinitebombs", "godmode", "undo noclip").
/// Bridges the Interpreter AST directly with the existing Command-based CheatService.
/// </summary>
public class CheatTerminalExpression : AbstractConsoleExpression
{
    private readonly string _cheatName;
    private readonly bool _isUndo;

    public CheatTerminalExpression(string cheatName, bool isUndo = false)
    {
        _cheatName = (cheatName ?? string.Empty).Trim().ToLowerInvariant();
        _isUndo = isUndo;
    }

    public override void Interpret(ConsoleContext context)
    {
        var target = context.TargetPlayer;

        if (_isUndo)
        {
            context.Cheats.UndoCheat(target.Id, _cheatName);
            context.AppendOutput($"[Cheat] Reverted '{_cheatName}' on {target.Name}.");
            return;
        }

        bool activated = context.Cheats.TryActivateCheat(target, _cheatName);
        if (activated)
        {
            context.AppendOutput($"[Cheat] Activated '{_cheatName}' for {target.Name}.");
        }
        else
        {
            context.AppendOutput($"[Cheat] '{_cheatName}' is already active or unknown for {target.Name}.");
        }
    }
}
