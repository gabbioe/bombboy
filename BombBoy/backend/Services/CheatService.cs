using BombBoy.Models;
using System.Collections.Generic;
using System.Linq;

namespace BombBoy.Services;

public sealed class CheatService
{
    private readonly Dictionary<string, List<ICommand>> _cheatHistory = new();

    public void ActivateCheat(Player player, string cheatCode)
    {
        ICommand? command = cheatCode.Trim().ToLower() switch
        {
            "noclip" => new CheatCode_PassWalls(player),
            "infinitebombs" => new CheatCode_InfiniteBombs(player),
            _ => null
        };

        if (command == null)
            return;

        if (!_cheatHistory.TryGetValue(player.Id, out var activeList))
        {
            activeList = new List<ICommand>();
            _cheatHistory[player.Id] = activeList;
        }

        if (activeList.Any(c => c.Name == command.Name))
            return;

        command.Execute();
        activeList.Add(command);
    }

    public void UndoCheat(string connectionId, string cheatCode)
    {
        string target = cheatCode.Trim().ToLower();
        if (!_cheatHistory.TryGetValue(connectionId, out var activeList))
            return;

        ICommand? command = activeList.FirstOrDefault(c => c.Name == target);
        if (command != null)
        {
            command.Undo();
            activeList.Remove(command);
        }
    }

    public void RemovePlayer(string connectionId) =>
        _cheatHistory.Remove(connectionId);

    public void Clear() =>
        _cheatHistory.Clear();

    public IReadOnlyDictionary<string, List<ICommand>> GetActiveCheats() =>
        _cheatHistory;
}
