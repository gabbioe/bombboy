using System;

namespace BombBoy.Models.Interpreter;

/// <summary>
/// 'NonterminalExpression' 2 (Rule: &lt;repeatExpr&gt; ::= "repeat" &lt;count&gt; &lt;expr&gt; ).
/// Holds a child AbstractConsoleExpression and recursively interprets it N times.
/// </summary>
public class RepeatNonterminalExpression : AbstractConsoleExpression
{
    private readonly int _count;
    private readonly AbstractConsoleExpression _expression;

    public RepeatNonterminalExpression(int count, AbstractConsoleExpression expression)
    {
        _count = Math.Clamp(count, 1, 10);
        _expression = expression ?? throw new ArgumentNullException(nameof(expression));
    }

    public override void Interpret(ConsoleContext context)
    {
        for (int i = 0; i < _count; i++)
        {
            _expression.Interpret(context);
        }
    }
}
