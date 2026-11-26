using System;
using System.Collections.Generic;
using System.Linq;

namespace BombBoy.Models.Interpreter;

/// <summary>
/// 'NonterminalExpression' 3 (Rule: &lt;scopedExpr&gt; ::= ("@all" | "@alive" | "@enemies" | "@me") &lt;expr&gt; ).
/// Resolves target players in ConsoleContext and delegates Interpret(context) to its child expression for each player.
/// </summary>
public class TargetScopeNonterminalExpression : AbstractConsoleExpression
{
    private readonly string _scope;
    private readonly AbstractConsoleExpression _expression;

    public TargetScopeNonterminalExpression(string scope, AbstractConsoleExpression expression)
    {
        _scope = (scope ?? "@me").Trim().ToLowerInvariant();
        _expression = expression ?? throw new ArgumentNullException(nameof(expression));
    }

    public override void Interpret(ConsoleContext context)
    {
        List<Player> targets = ResolveTargets(context);
        if (targets.Count == 0)
        {
            context.AppendOutput($"[Warn] No players matched scope '{_scope}'.");
            return;
        }

        Player previousTarget = context.TargetPlayer;
        try
        {
            foreach (var target in targets)
            {
                context.TargetPlayer = target;
                _expression.Interpret(context);
            }
        }
        finally
        {
            context.TargetPlayer = previousTarget;
        }
    }

    private List<Player> ResolveTargets(ConsoleContext context)
    {
        var allPlayers = context.Match.GetAllPlayersList();
        return _scope switch
        {
            "@all" => allPlayers,
            "@alive" => allPlayers.Where(p => p.IsAlive).ToList(),
            "@enemies" => allPlayers.Where(p => p.Id != context.Caller.Id && p.IsAlive).ToList(),
            "@me" => new List<Player> { context.Caller },
            _ => allPlayers
                .Where(p => p.Name.Contains(_scope.TrimStart('@'), StringComparison.OrdinalIgnoreCase))
                .ToList()
        };
    }
}
