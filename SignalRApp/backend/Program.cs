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
maap.DoDamange(2, 2);
Console.WriteLine(maap.ReturnMap());


app.Run();
