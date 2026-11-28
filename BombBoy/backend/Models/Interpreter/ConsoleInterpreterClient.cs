using System;
using System.Collections.Generic;

namespace BombBoy.Models.Interpreter;

/// <summary>
/// The 'Client' participant in the DoFactory Interpreter pattern.
/// Builds the Abstract Syntax Tree (AST) of TerminalExpression and NonterminalExpression
/// instances from ConsoleContext.Input and invokes Interpret(context).
/// </summary>
public class ConsoleInterpreterClient
{
    private static readonly HashSet<string> KnownCheats = new(StringComparer.OrdinalIgnoreCase)
    {
        "noclip",
        "infinitebombs",
        "godmode"
    };

    public string Execute(ConsoleContext context)
    {
        ArgumentNullException.ThrowIfNull(context);

        AbstractConsoleExpression syntaxTree = BuildSyntaxTree(context.Input);
        syntaxTree.Interpret(context);

        string result = context.Output;
        Console.WriteLine($"[Interpreter] {context.Caller.Name} > \"{context.Input}\" => {result.Replace('\n', ' ')}");
        return result;
    }

    public AbstractConsoleExpression BuildSyntaxTree(string rawInput)
    {
        if (string.IsNullOrWhiteSpace(rawInput))
            return new ArenaCommandTerminalExpression("help");

        string[] segments = rawInput.Split(';', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
        if (segments.Length == 0)
            return new ArenaCommandTerminalExpression("help");

        if (segments.Length == 1)
            return ParseStatement(segments[0]);

        var sequence = new SequenceNonterminalExpression();
        foreach (string segment in segments)
        {
            sequence.Add(ParseStatement(segment));
        }
        return sequence;
    }

    private AbstractConsoleExpression ParseStatement(string statement)
    {
        string trimmed = statement.Trim().TrimStart('/');
        string[] tokens = trimmed.Split(' ', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
        if (tokens.Length == 0)
            return new ArenaCommandTerminalExpression("help");

        return ParseTokens(tokens, 0);
    }

    private AbstractConsoleExpression ParseTokens(string[] tokens, int index)
    {
        if (index >= tokens.Length)
            return new ArenaCommandTerminalExpression("help");

        string head = tokens[index].ToLowerInvariant();

        // Nonterminal 1: repeat <N> <statement>
        if (head == "repeat" && index + 2 < tokens.Length && int.TryParse(tokens[index + 1], out int repeatCount))
        {
            AbstractConsoleExpression inner = ParseTokens(tokens, index + 2);
            return new RepeatNonterminalExpression(repeatCount, inner);
        }

        // Nonterminal 2: @scope <statement>  (e.g., @all give range 5, @enemies freeze)
        if (head.StartsWith('@') && index + 1 < tokens.Length)
        {
            AbstractConsoleExpression inner = ParseTokens(tokens, index + 1);
            return new TargetScopeNonterminalExpression(head, inner);
        }

        // Terminal 1a: undo <cheat>
        if (head == "undo" && index + 1 < tokens.Length)
        {
            return new CheatTerminalExpression(tokens[index + 1], isUndo: true);
        }

        // Terminal 1b: cheat <name>  OR  bare cheat keyword (noclip / infinitebombs / godmode)
        if (head == "cheat" && index + 1 < tokens.Length)
        {
            return new CheatTerminalExpression(tokens[index + 1], isUndo: false);
        }

        if (KnownCheats.Contains(head))
        {
            return new CheatTerminalExpression(head, isUndo: false);
        }

        // Terminal 2: give [optional @scope] <stat> <amount>
        if (head == "give")
        {
            if (index + 2 < tokens.Length && tokens[index + 1].StartsWith('@'))
            {
                string scope = tokens[index + 1];
                string stat = index + 2 < tokens.Length ? tokens[index + 2] : "";
                int val = (index + 3 < tokens.Length && int.TryParse(tokens[index + 3], out int parsedScoped)) ? parsedScoped : 1;
                return new TargetScopeNonterminalExpression(scope, new GiveStatTerminalExpression(stat, val));
            }

            string statName = index + 1 < tokens.Length ? tokens[index + 1] : "";
            int amount = (index + 2 < tokens.Length && int.TryParse(tokens[index + 2], out int parsed)) ? parsed : 1;
            return new GiveStatTerminalExpression(statName, amount);
        }

        // Terminal 3: spawn <fusion|weapon|powerup|shield|bomb>
        if (head == "spawn")
        {
            string kind = index + 1 < tokens.Length ? tokens[index + 1] : "powerup";
            return new SpawnTerminalExpression(kind);
        }

        // Terminal 4: arena / round / strategy / state commands
        string arg = index + 1 < tokens.Length ? string.Join(' ', tokens[(index + 1)..]) : string.Empty;
        return new ArenaCommandTerminalExpression(head, arg);
    }
}
