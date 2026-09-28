using Microsoft.AspNetCore.SignalR;
using SignalRApp.Services;

public class AppHub : Hub
{
    private readonly GameManager _gameManager;

    public AppHub(GameManager gameManager)
    {
        _gameManager = gameManager;
    }

    public override async Task OnConnectedAsync()
    {
        var newPlayer = _gameManager.AddPlayer(Context.ConnectionId);

        await Clients.Caller.SendAsync("ReceiveMap", _gameManager.GetMap());
        await Clients.Caller.SendAsync("ReceivePlayerId", Context.ConnectionId);
        await Clients.All.SendAsync("UpdatePlayers", _gameManager.GetAllPlayers());
        await base.OnConnectedAsync();
    }

    public override async Task OnDisconnectedAsync(Exception? exception)
    {
        if (_gameManager.RemovePlayer(Context.ConnectionId, out _))
        {
            await Clients.All.SendAsync("UpdatePlayers", _gameManager.GetAllPlayers());
            await Clients.All.SendAsync("ReceiveMap", _gameManager.GetMap());
        }
        await base.OnDisconnectedAsync(exception);
    }

    public async Task Move(int dx, int dy)
    {
        _gameManager.MovePlayer(Context.ConnectionId, dx, dy);
        await Clients.All.SendAsync("UpdatePlayers", _gameManager.GetAllPlayers());
    }

    public async Task PlaceBomb(string? strategy = null)
    {
        bool placed = _gameManager.PlaceBomb(Context.ConnectionId, strategy);
        if (placed)
        {
            await Clients.All.SendAsync("ReceiveMap", _gameManager.GetMap());
        }
    }

    public void SetStrategy(string strategy)
    {
        _gameManager.SetPlayerStrategy(Context.ConnectionId, strategy);
    }

    public string GetMap()
    {
        return _gameManager.GetMap();
    }
}