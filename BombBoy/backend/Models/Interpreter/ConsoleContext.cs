using System;
using System.Collections.Generic;
using BombBoy.Services;

namespace BombBoy.Models.Interpreter;

/// <summary>
/// The 'Context' participant in the DoFactory Interpreter pattern.
/// Contains global information and input/output state external to the interpreter.
/// </summary>
public class ConsoleContext
{
    private readonly List<string> _outputLines = new();

    public string Input { get; set; }
    public string Output
    {
        get => string.Join("\n", _outputLines);
        set
        {
            _outputLines.Clear();
            if (!string.IsNullOrEmpty(value))
                _outputLines.Add(value);
        }
    }

    public Player Caller { get; }
    public Player TargetPlayer { get; set; }
    public Match Match { get; }
    public RoundService Round { get; }
    public CheatService Cheats { get; }

    public ConsoleContext(
        string input,
        Player caller,
        Match match,
        RoundService round,
        CheatService cheats)
    {
        Input = input ?? string.Empty;
        Caller = caller ?? throw new ArgumentNullException(nameof(caller));
        TargetPlayer = caller;
        Match = match ?? throw new ArgumentNullException(nameof(match));
        Round = round ?? throw new ArgumentNullException(nameof(round));
        Cheats = cheats ?? throw new ArgumentNullException(nameof(cheats));
    }

    public void AppendOutput(string message)
    {
        if (!string.IsNullOrWhiteSpace(message))
        {
            _outputLines.Add(message);
        }
    }
}
