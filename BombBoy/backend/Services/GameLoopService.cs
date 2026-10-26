using Microsoft.AspNetCore.SignalR;

namespace BombBoy.Services;

public class GameLoopService : BackgroundService
{
    private readonly GameManager _gameManager;
    private readonly IHubContext<AppHub> _hubContext;

    public GameLoopService(GameManager gameManager, IHubContext<AppHub> hubContext)
    {
        _gameManager = gameManager;
        _hubContext = hubContext;

        _gameManager.OnSoundEffectPlayed += (soundFile, volume) =>
        {
            _ = _hubContext.Clients.All.SendAsync("PlaySoundEffect", soundFile, volume);
        };
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        const int tickrate = 50;
        const float deltaTime = 0.05f;

        //Asuminu cia vyksta viena karta tik kai paleidi game, jei ne tai negerai :/
        _gameManager.GenerateMap();
        while (!stoppingToken.IsCancellationRequested)
        {

            if (_gameManager.GetAllPlayers().Any())
            {
                _gameManager.Update(deltaTime);
                await _hubContext.Clients.All.SendAsync("Gametick", _gameManager.GetFullMap(), stoppingToken);
            }

            await Task.Delay(tickrate, stoppingToken);
            
        }
    }
}
