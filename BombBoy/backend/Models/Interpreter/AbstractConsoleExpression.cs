namespace BombBoy.Models.Interpreter;

/// <summary>
/// The 'AbstractExpression' participant in the DoFactory Interpreter pattern.
/// Declares an abstract Interpret(ConsoleContext) operation common to all nodes in the AST.
/// </summary>
public abstract class AbstractConsoleExpression
{
    public abstract void Interpret(ConsoleContext context);
}
