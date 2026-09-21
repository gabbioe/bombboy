using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using SignalRApp.Services;

// Inheriting from "Hub" gives this class superpowers:
// 1. Clients (browsers) can call public methods on this class directly via JavaScript.
// 2. This class can push messages/data back to connected clients in real time.
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

       await Clients.All.SendAsync("UpdatePlayers", _gameManager.GetAllPlayers());
       await base.OnConnectedAsync();
    }

    // Any public method you write here can be called directly from JavaScript in the browser!
    // Example in client JS: connection.invoke("SendUserAction", "jump", 42);

    public override async Task OnDisconnectedAsync(Exception? exception)                                         
    {                                                                                                            
        // Remove player when they disconnect                                                                    
        if (_gameManager.RemovePlayer(Context.ConnectionId, out _))                                              
        {                                                                                                        
            await Clients.All.SendAsync("UpdatePlayers", _gameManager.GetAllPlayers());                          
        }                                                                                                        
                                                                                                                     
        await base.OnDisconnectedAsync(exception);                                                               
    }        
    public async Task Move(float dx, float dy)
    {
        _gameManager.MovePlayer(Context.ConnectionId, dx, dy);
        await Clients.All.SendAsync("UpdatePlayers", _gameManager.GetAllPlayers());
    }
}