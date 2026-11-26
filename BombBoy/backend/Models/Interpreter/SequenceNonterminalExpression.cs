using System.Collections.Generic;

namespace BombBoy.Models.Interpreter;

/// <summary>
/// 'NonterminalExpression' 1 (Rule: &lt;commandLine&gt; ::= &lt;expr&gt; ( ";" &lt;expr&gt; )* ).
/// Aggregates multiple child AbstractConsoleExpression instances and interprets them sequentially.
/// </summary>
public class SequenceNonterminalExpression : AbstractConsoleExpression
{
    private readonly List<AbstractConsoleExpression> _expressions = new();

    public SequenceNonterminalExpression(IEnumerable<AbstractConsoleExpression>? expressions = null)
    {
        if (expressions != null)
        {
            _expressions.AddRange(expressions);
        }
    }

    public void Add(AbstractConsoleExpression expression)
    {
        _expressions.Add(expression);
    }

    public IReadOnlyList<AbstractConsoleExpression> Expressions => _expressions;

    public override void Interpret(ConsoleContext context)
    {
        foreach (var expression in _expressions)
        {
            expression.Interpret(context);
        }
    }
}
