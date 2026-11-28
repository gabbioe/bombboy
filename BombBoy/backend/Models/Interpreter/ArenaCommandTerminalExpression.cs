using System;
using BombBoy.Models.State;
using BombBoy.Services;

namespace BombBoy.Models.Interpreter;

/// <summary>
/// 'TerminalExpression' 4: Interprets arena, round, strategy, and condition commands
/// (e.g., "clear 10", "time +30", "strategy bullseye", "freeze", "heal", "help").
/// </summary>
public class ArenaCommandTerminalExpression : AbstractConsoleExpression
{
    private readonly string _action;
    private readonly string _argument;

    public ArenaCommandTerminalExpression(string action, string argument = "")
    {
        _action = (action ?? string.Empty).Trim().ToLowerInvariant();
        _argument = (argument ?? string.Empty).Trim();
    }

    public override void Interpret(ConsoleContext context)
    {
        switch (_action)
        {
            case "help":
                context.AppendOutput(
                    "Chat Mediator: /@name <msg> | /bounty [name] | /status\n" +
                    "Interpreter: /noclip | /infinitebombs | /godmode | /undo <cheat> | " +
                    "/give <range|bombs|lives|shield|speed> <N> | " +
                    "/spawn <fusion|weapon|powerup|shield|bomb> | " +
                    "/strategy <name> | /clear [N] | /time <+S|S> | /freeze | /heal | " +
                    "Combinators: <cmd1> ; <cmd2> | /repeat <N> <cmd> | /@all /@enemies /@alive <cmd>");
                break;

            case "clear":
                int maxBlocks = int.TryParse(_argument, out int parsedMax) ? Math.Max(1, parsedMax) : 999;
                int destroyed = context.Match.ClearDestructibleBlocks(maxBlocks);
                context.AppendOutput($"[Arena] Cleared {destroyed} destructible block(s).");
                break;

            case "time":
                if (string.IsNullOrEmpty(_argument))
                {
                    context.AppendOutput($"[Round] Remaining time: {Math.Ceiling(context.Round.RemainingSeconds)}s.");
                    break;
                }
                if ((_argument.StartsWith('+') || _argument.StartsWith('-')) && float.TryParse(_argument, out float delta))
                {
                    context.Round.AdjustRemainingTime(delta);
                    context.AppendOutput($"[Round] Adjusted timer by {delta:+0;-0}s -> {Math.Ceiling(context.Round.RemainingSeconds)}s.");
                }
                else if (float.TryParse(_argument, out float exactSeconds))
                {
                    context.Round.SetRemainingTime(exactSeconds);
                    context.AppendOutput($"[Round] Set timer to {Math.Ceiling(context.Round.RemainingSeconds)}s.");
                }
                else
                {
                    context.AppendOutput("[Error] Usage: time +30  or  time 90");
                }
                break;

            case "strategy":
            case "strat":
                if (string.IsNullOrWhiteSpace(_argument))
                {
                    context.AppendOutput("[Error] Usage: strategy <classic|ring|knight|checker|bullseye|grandmaster|biochecker|doublering>");
                    break;
                }
                var resolved = PlayerService.ResolveStrategy(_argument);
                context.TargetPlayer.ApplyPowerUp(resolved);
                context.AppendOutput($"[Strategy] {context.TargetPlayer.Name} bomb set to '{_argument}'.");
                break;

            case "freeze":
                context.TargetPlayer.ApplyFrost(3.0f);
                context.AppendOutput($"[State] Froze {context.TargetPlayer.Name} for 3s.");
                break;

            case "heal":
            case "thaw":
                if (context.TargetPlayer.IsAlive)
                {
                    context.TargetPlayer.TransitionToState(new HealthyState());
                    context.TargetPlayer.Lives = Math.Max(context.TargetPlayer.Lives, 3);
                    context.AppendOutput($"[State] Restored {context.TargetPlayer.Name} to Healthy (Lives: {context.TargetPlayer.Lives}).");
                }
                else
                {
                    context.AppendOutput($"[Warn] {context.TargetPlayer.Name} is already eliminated this round.");
                }
                break;

            default:
                context.AppendOutput($"[Error] Unrecognized command '{_action}'. Type 'help' for syntax.");
                break;
        }
    }
}
