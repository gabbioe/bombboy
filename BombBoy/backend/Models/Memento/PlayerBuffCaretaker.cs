using System;
using System.Collections.Generic;

namespace BombBoy.Models.Memento;

/// <summary>
/// Caretaker in the Memento pattern.
/// Stores baseline (unbuffed) mementos and pre-buff checkpoint mementos for players
/// using the narrow IPlayerMemento interface, without accessing Player's internal fields.
/// </summary>
public class PlayerBuffCaretaker
{
    private readonly Dictionary<string, IPlayerMemento> _baselineMementos = new();
    private readonly Dictionary<string, Stack<IPlayerMemento>> _buffHistory = new();

    /// <summary>
    /// Captures and stores the player's clean baseline state (unbuffed).
    /// </summary>
    public void SaveBaseline(Player player)
    {
        if (player == null) throw new ArgumentNullException(nameof(player));

        _baselineMementos[player.Id] = player.CreateMemento("Baseline");
        _buffHistory[player.Id] = new Stack<IPlayerMemento>();
    }

    /// <summary>
    /// Saves a checkpoint snapshot of the player right before a new buff or item is picked up.
    /// </summary>
    public void SavePreBuffCheckpoint(Player player, string buffLabel)
    {
        if (player == null) throw new ArgumentNullException(nameof(player));

        if (!_buffHistory.TryGetValue(player.Id, out var history))
        {
            history = new Stack<IPlayerMemento>();
            _buffHistory[player.Id] = history;
        }

        history.Push(player.CreateMemento(buffLabel));
    }

    /// <summary>
    /// Restores the player back to their clean baseline Memento (clearing all buffs, weapons, and decorators).
    /// </summary>
    public void ClearAllBuffs(Player player)
    {
        if (player == null) throw new ArgumentNullException(nameof(player));

        if (!_baselineMementos.TryGetValue(player.Id, out var baseline))
        {
            // If no baseline was recorded yet, reset decorators and record baseline now
            player.ResetDecorators();
            SaveBaseline(player);
            return;
        }

        player.RestoreMemento(baseline);

        if (_buffHistory.TryGetValue(player.Id, out var history))
        {
            history.Clear();
        }
    }

    /// <summary>
    /// Restores the most recent pre-buff checkpoint Memento (rolling back the last buff picked up).
    /// </summary>
    public bool UndoLastBuff(Player player)
    {
        if (player == null) throw new ArgumentNullException(nameof(player));

        if (_buffHistory.TryGetValue(player.Id, out var history) && history.Count > 0)
        {
            var previousMemento = history.Pop();
            player.RestoreMemento(previousMemento);
            return true;
        }

        return false;
    }

    public void RemovePlayer(string playerId)
    {
        _baselineMementos.Remove(playerId);
        _buffHistory.Remove(playerId);
    }
}
