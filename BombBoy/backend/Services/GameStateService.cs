using BombBoy.Models;
using BombBoy.Models.Observer;
using System;
using System.Text.Json.Nodes;

namespace BombBoy.Services;

public sealed class GameStateService
{
    public string BuildFullMapJson(
        Match match,
        RoundService round,
        LobbyService lobby,
        LeaderboardObserver leaderboard,
        CheatService cheats)
    {
        string matchMapJson = match.GetFullGameMapJson();
        JsonObject root;
        try
        {
            root = JsonNode.Parse(matchMapJson)?.AsObject() ?? new JsonObject();
        }
        catch
        {
            root = new JsonObject();
        }

        root["Stage"] = round.Stage.ToString();
        root["CurrentRound"] = round.CurrentRound;
        root["TotalRounds"] = round.TotalRounds;
        root["RemainingSeconds"] = (int)Math.Ceiling(round.RemainingSeconds);
        root["RoundDuration"] = round.RoundDurationSeconds;
        root["IntermissionCountdown"] = (int)Math.Ceiling(round.IntermissionCountdown);
        root["SelectedMapType"] = round.SelectedMapType;
        root["RoundWinner"] = round.RoundWinnerName;
        root["Champion"] = round.ChampionName;
        root["SameMapVotes"] = round.SameMapVotesCount;
        root["SameMapVotesNeeded"] = round.GetSameMapVotesNeeded(lobby.PlayerCount);

        var activeCheatsObj = new JsonObject();
        foreach (var kvp in cheats.GetActiveCheats())
        {
            var arr = new JsonArray();
            foreach (var cmd in kvp.Value)
            {
                arr.Add(cmd.Name);
            }
            activeCheatsObj[kvp.Key] = arr;
        }
        root["ActiveCheats"] = activeCheatsObj;

        var standings = leaderboard.GetStandings();
        var lbArray = new JsonArray();
        foreach (var s in standings)
        {
            bool isHost = lobby.IsHost(s.PlayerId);
            lbArray.Add(new JsonObject
            {
                ["PlayerId"] = s.PlayerId,
                ["PlayerName"] = s.PlayerName,
                ["IsHost"] = isHost,
                ["Kills"] = s.Kills,
                ["Deaths"] = s.Deaths,
                ["RoundWins"] = s.RoundWins,
                ["Score"] = s.Score
            });
        }
        root["Leaderboard"] = lbArray;

        var playersArray = new JsonArray();
        foreach (var lp in lobby.GetPlayers())
        {
            playersArray.Add(new JsonObject
            {
                ["Id"] = lp.Id,
                ["Name"] = lp.Name,
                ["IsHost"] = lp.IsHost,
                ["IsReady"] = lp.IsReady,
                ["MapVote"] = lp.MapVote
            });
        }

        root["Lobby"] = new JsonObject
        {
            ["Players"] = playersArray,
            ["PlayerCount"] = lobby.PlayerCount,
            ["TotalRounds"] = round.TotalRounds,
            ["RoundDurationSeconds"] = round.RoundDurationSeconds,
            ["CanForceStart"] = lobby.CanForceStart(),
            ["CanAutoStart"] = lobby.CanAutoStart
        };

        return root.ToJsonString();
    }
}
