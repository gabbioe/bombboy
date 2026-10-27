namespace BombBoy.Services;

public sealed class LobbyService
{
    private const int MaximumPlayers = 4;
    private readonly Dictionary<string, LobbyPlayerState> _players = new();

    public int PlayerCount => _players.Count;

    public bool TryAddPlayer(string id, string name)
    {
        if (_players.Count >= MaximumPlayers)
            return false;

        _players[id] = new LobbyPlayerState
        {
            Id = id,
            Name = name,
            IsHost = _players.Count == 0,
            IsReady = false,
            MapVote = "normal"
        };

        return true;
    }

    public bool RemovePlayer(string id)
    {
        if (!_players.Remove(id, out var removedPlayer))
            return false;

        if (removedPlayer.IsHost && _players.Count > 0)
            _players.Values.First().IsHost = true;

        return true;
    }

    public bool TryToggleReady(string id)
    {
        if (!_players.TryGetValue(id, out var player))
            return false;

        player.IsReady = !player.IsReady;
        return true;
    }

    public bool CanForceStart(string id) =>
        IsHost(id) && CanForceStart();

    public bool CanForceStart()
    {
        if (_players.Count < 2)
            return false;

        int readyCount = _players.Values.Count(player => player.IsReady);
        int requiredReadyCount = (int)Math.Ceiling(_players.Count / 2.0);
        return readyCount >= requiredReadyCount;
    }

    public bool IsHost(string id) =>
        _players.TryGetValue(id, out var player) && player.IsHost;

    public bool TryVoteMap(string id, string mapType)
    {
        if (!_players.TryGetValue(id, out var player))
            return false;

        player.MapVote = mapType.ToLower() == "ice" ? "ice" : "normal";
        return true;
    }

    public string ChooseMapType()
    {
        int normalVotes = _players.Values.Count(player => player.MapVote == "normal");
        int iceVotes = _players.Values.Count(player => player.MapVote == "ice");

        if (normalVotes > iceVotes)
            return "normal";
        if (iceVotes > normalVotes)
            return "ice";

        return Random.Shared.Next(0, 2) == 0 ? "normal" : "ice";
    }

    public IReadOnlyList<LobbyPlayerSnapshot> GetPlayers() =>
        _players.Values
            .Select(player => new LobbyPlayerSnapshot(
                player.Id,
                player.Name,
                player.IsHost,
                player.IsReady,
                player.MapVote))
            .ToList();

    public bool CanAutoStart =>
        _players.Count >= 2 && _players.Values.All(player => player.IsReady);

    public void ResetReadyStatus()
    {
        foreach (var player in _players.Values)
            player.IsReady = false;
    }

    private sealed class LobbyPlayerState
    {
        public string Id { get; init; } = string.Empty;
        public string Name { get; init; } = string.Empty;
        public bool IsHost { get; set; }
        public bool IsReady { get; set; }
        public string MapVote { get; set; } = "normal";
    }
}

public sealed record LobbyPlayerSnapshot(
    string Id,
    string Name,
    bool IsHost,
    bool IsReady,
    string MapVote);
