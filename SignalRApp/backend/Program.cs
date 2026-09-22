using SignalRApp.Services;
using SignalRApp.Models;

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
/*
var maap = new Map(7, 7,0);
var p1 = new Player();
p1.Id = "P1";
var p2 = new Player();
p2.Id = "P2";
maap.AddPlayer(p1);
maap.AddPlayer(p2);
var p3 = new Player();
p3.Id = "P3";
var p4 = new Player();
p4.Id = "P4";
maap.AddPlayer(p3);
maap.AddPlayer(p4);
Console.WriteLine(maap.ReturnMapToConsole());
Console.WriteLine();
maap.Move(1, 0, p1.Id);
maap.Move(1, 0, p1.Id);
maap.Move(1, 0, p2.Id);
maap.Move(1, 0, p2.Id);
maap.Move(0, 1, p3.Id);
maap.Move(0, 1, p3.Id);
maap.Move(0, -1, p4.Id);
maap.Move(0, -1, p4.Id);

Console.WriteLine(maap.ReturnMapToConsole());
*/



app.Run();
