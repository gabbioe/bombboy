using SignalRApp.Services;
using SignalRApp.Models;
using SignalRApp.Models.TileFactories;

var builder = WebApplication.CreateBuilder(args);


var frontendPath = Path.GetFullPath(Path.Combine(builder.Environment.ContentRootPath, "..", "frontend", "wwwroot"));
builder.Environment.WebRootPath = frontendPath;
builder.Environment.WebRootFileProvider = new Microsoft.Extensions.FileProviders.PhysicalFileProvider(frontendPath);


builder.Services.AddSignalR();

builder.Services.AddSingleton<GameManager>();

builder.Services.AddHostedService<GameLoopService>();


var app = builder.Build();

app.UseDefaultFiles();

app.UseStaticFiles();

app.MapHub<AppHub>("/hub");

Match DaMatch = new Match();
DaMatch.GenerateMap(7, 7, 40, new BasicMapTileFactory());
var p1 = new Player();
p1.Id = "P1";

var p2 = new Player();
p2.Id = "P2";
DaMatch.AddPlayer(p1);
DaMatch.AddPlayer(p2);
DaMatch.MatchHasStarted = true;
Console.WriteLine();
Console.WriteLine(DaMatch.GetFullGameMapJson(true));
Console.WriteLine();
DaMatch.Move(1, 0, p1.Id);
DaMatch.Move(1, 0, p1.Id);
DaMatch.Move(0, 1, p1.Id);
DaMatch.Move(1, 0, p2.Id);
DaMatch.Move(1, 0, p2.Id);
DaMatch.Move(0, -1, p1.Id);
Console.WriteLine();
Console.WriteLine(DaMatch.GetFullGameMapJson(false));
Console.WriteLine();



app.Run();
