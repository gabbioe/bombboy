using Microsoft.AspNetCore.SignalR;
using BombBoy.Services;
using System.Threading.Tasks;

public class AppHub : Hub
{
    private readonly GameManager _gameManager;

    public AppHub(GameManager gameManager)
    {
        _gameManager = gameManager;
    }

    public override async Task OnConnectedAsync()
    {
        var result = _gameManager.AddPlayer(Context.ConnectionId);
        if (result.isFull)
        {
            await Clients.Caller.SendAsync("LobbyFull", "The lobby is currently full (maximum 4 players allowed). Please try again later.");
            return;
        }

        await Clients.Caller.SendAsync("ReceiveMap", _gameManager.GetFullMap());
        await Clients.Caller.SendAsync("ReceivePlayerId", Context.ConnectionId);
        await Clients.All.SendAsync("UpdatePlayers", _gameManager.GetAllPlayers());
        await Clients.All.SendAsync("Gametick", _gameManager.GetFullMap());
        await base.OnConnectedAsync();
    }

    public override async Task OnDisconnectedAsync(Exception? exception)
    {
        if (_gameManager.RemovePlayer(Context.ConnectionId, out _))
        {
            await Clients.All.SendAsync("UpdatePlayers", _gameManager.GetAllPlayers());
            await Clients.All.SendAsync("Gametick", _gameManager.GetFullMap());
        }

        await base.OnDisconnectedAsync(exception);
    }

    public async Task ToggleReady()
    {
        _gameManager.ToggleReady(Context.ConnectionId);
        await Clients.All.SendAsync("Gametick", _gameManager.GetFullMap());
    }

    public async Task VoteMap(string mapType)
    {
        _gameManager.VoteMap(Context.ConnectionId, mapType);
        await Clients.All.SendAsync("Gametick", _gameManager.GetFullMap());
    }

    public async Task VoteSameMap()
    {
        _gameManager.VoteSameMap(Context.ConnectionId);
        await Clients.All.SendAsync("Gametick", _gameManager.GetFullMap());
    }

    public async Task SetMatchRounds(int rounds)
    {
        _gameManager.SetMatchRounds(Context.ConnectionId, rounds);
        await Clients.All.SendAsync("Gametick", _gameManager.GetFullMap());
    }

    public async Task SetMatchDuration(int durationSeconds)
    {
        _gameManager.SetMatchDuration(Context.ConnectionId, durationSeconds);
        await Clients.All.SendAsync("Gametick", _gameManager.GetFullMap());
    }

    public async Task ForceStartGame()
    {
        _gameManager.ForceStartGame(Context.ConnectionId);
        await Clients.All.SendAsync("Gametick", _gameManager.GetFullMap());
    }

    public async Task ReturnToLobby()
    {
        _gameManager.ReturnToLobby();
        await Clients.All.SendAsync("Gametick", _gameManager.GetFullMap());
    }

    public async Task UseWeapon()
    {
        _gameManager.UseWeapon(Context.ConnectionId);
        await Clients.All.SendAsync("Gametick", _gameManager.GetFullMap());
    }

    public async Task Move(int dx, int dy)
    {
        _gameManager.MovePlayer(Context.ConnectionId, dx, dy);
        await Clients.All.SendAsync("Gametick", _gameManager.GetFullMap());
    }

    public string GetMap()
    {
        return _gameManager.GetFullMap();
    }

    public void PlaceBomb(string? strategyName = null)
    {
        _gameManager.PlaceBomb(Context.ConnectionId, strategyName);
    }

    public void SetStrategy(string strategyName)
    {
        _gameManager.SetPlayerStrategy(Context.ConnectionId, strategyName);
    }
}