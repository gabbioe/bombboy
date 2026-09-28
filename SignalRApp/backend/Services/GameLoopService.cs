using Microsoft.AspNetCore.SignalR;

namespace SignalRApp.Services;

public class GameLoopService : BackgroundService
{
    private readonly GameManager _gameManager;
    private readonly IHubContext<AppHub> _hubContext;

    public GameLoopService(GameManager gameManager, IHubContext<AppHub> hubContext)
    {
        _gameManager = gameManager;
        _hubContext = hubContext;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        const int tickrate = 50;

        while (!stoppingToken.IsCancellationRequested)
        {
            bool mapChanged = _gameManager.UpdateGameState();

            if (_gameManager.GetAllPlayers().Any())
            {
                if (mapChanged)
                {
                    await _hubContext.Clients.All.SendAsync("ReceiveMap", _gameManager.GetMap(), stoppingToken);
                }

                await _hubContext.Clients.All.SendAsync("Gametick", _gameManager.GetAllPlayers(), stoppingToken);
            }

            await Task.Delay(tickrate, stoppingToken);
        }
    }
}