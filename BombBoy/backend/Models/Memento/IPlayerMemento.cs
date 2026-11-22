using System;

namespace BombBoy.Models.Memento;

/// <summary>
/// Narrow Memento interface exposed to the Caretaker (PlayerBuffCaretaker).
/// Exposes only snapshot metadata so the Caretaker cannot inspect or mutate the Originator's (Player's) internal state.
/// </summary>
public interface IPlayerMemento
{
    string PlayerId { get; }
    string Label { get; }
    DateTime CreatedAt { get; }
}
