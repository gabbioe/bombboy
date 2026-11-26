using System;
using BombBoy.Models.Decorator;

namespace BombBoy.Models.Interpreter;

/// <summary>
/// 'TerminalExpression' 2: Interprets stat and buff grant commands
/// (e.g., "give range 5", "give bombs 4", "give lives 3", "give shield 15", "give speed 1").
/// </summary>
public class GiveStatTerminalExpression : AbstractConsoleExpression
{
    private readonly string _statName;
    private readonly int _amount;

    public GiveStatTerminalExpression(string statName, int amount)
    {
        _statName = (statName ?? string.Empty).Trim().ToLowerInvariant();
        _amount = amount;
    }

    public override void Interpret(ConsoleContext context)
    {
        var target = context.TargetPlayer;

        switch (_statName)
        {
            case "range":
            case "bombrange":
                target.BombRange = Math.Clamp(_amount, 1, 9);
                context.AppendOutput($"[Stat] {target.Name} BombRange = {target.BombRange}.");
                break;

            case "bombs":
            case "bomb":
            case "bombcount":
                target.BombCount = Math.Clamp(_amount, 1, 99);
                context.AppendOutput($"[Stat] {target.Name} BombCount = {target.BombCount}.");
                break;

            case "lives":
            case "life":
            case "hp":
                target.Lives = Math.Clamp(_amount, 1, 9);
                context.AppendOutput($"[Stat] {target.Name} Lives = {target.Lives}.");
                break;

            case "shield":
                int sec = Math.Clamp(_amount, 1, 60);
                target.Decorate(c => new ImmortalDecorator(c, sec));
                target.ShieldDuration = sec;
                context.AppendOutput($"[Buff] Granted {sec}s Shield to {target.Name}.");
                break;

            case "speed":
                int bonus = Math.Clamp(_amount, 1, 3);
                target.Decorate(c => new SpeedDecorator(c, bonus));
                context.AppendOutput($"[Buff] Granted +{bonus} Speed to {target.Name}.");
                break;

            default:
                context.AppendOutput($"[Error] Unknown stat '{_statName}'. Try: range, bombs, lives, shield, speed.");
                break;
        }
    }
}
