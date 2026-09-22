using SignalRApp.Services;
using SignalRApp.Models;

var builder = WebApplication.CreateBuilder(args);


var frontendPath = Path.GetFullPath(Path.Combine(builder.Environment.ContentRootPath, "..", "frontend", "wwwroot"));
builder.Environment.WebRootPath = frontendPath;
builder.Environment.WebRootFileProvider = new Microsoft.Extensions.FileProviders.PhysicalFileProvider(frontendPath);


builder.Services.AddSignalR();

builder.Services.AddSingleton<GameManager>();

var app = builder.Build();

app.UseDefaultFiles();

app.UseStaticFiles();

app.MapHub<AppHub>("/hub");

var maap = new Map(7, 7,10);
var p1 = new Player();
p1.Id = "P1";
maap.AddPlayer(p1);
Console.WriteLine(maap.ReturnMapToJSon());
Console.WriteLine();
maap.Move(1, 0, p1.Id);
Console.WriteLine(maap.ReturnMapToJSon());




app.Run();
