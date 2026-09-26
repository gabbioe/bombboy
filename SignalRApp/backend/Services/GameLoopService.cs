using Microsoft.AspNetCore.SignalR;
using SignalRApp.Models.TileFactories;

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

        //Asuminu cia vyksta viena karta tik kai paleidi game, jei ne tai negerai :/
        _gameManager.GenerateMap(7, 7, 50);
        while (!stoppingToken.IsCancellationRequested)
        {

            if (_gameManager.GetAllPlayers().Any())
            {
                await _hubContext.Clients.All.SendAsync("Gametick", _gameManager.GetAllPlayers(), stoppingToken);
            }

            await Task.Delay(tickrate, stoppingToken);
            
        }
    }
}