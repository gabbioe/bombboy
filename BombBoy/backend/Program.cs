using BombBoy.Benchmarks;
using BombBoy.Services;
using BombBoy.Services.Proxy;

if (args.Contains("--benchmark"))
{
    FlyweightBenchmark.Run();
    ProxyBenchmark.Run();
    return;
}

var builder = WebApplication.CreateBuilder(args);

var frontendPath = Path.GetFullPath(Path.Combine(builder.Environment.ContentRootPath, "..", "frontend", "wwwroot"));
builder.Environment.WebRootPath = frontendPath;
builder.Environment.WebRootFileProvider = new Microsoft.Extensions.FileProviders.PhysicalFileProvider(frontendPath);

builder.Services.AddSignalR();
builder.Services.AddSingleton<GameManager>();
builder.Services.AddSingleton<IGameManager>(sp =>
    new GameManagerProtectionProxy(sp.GetRequiredService<GameManager>()));
builder.Services.AddHostedService<GameLoopService>();

var app = builder.Build();

app.UseDefaultFiles();
app.UseStaticFiles();
app.MapHub<AppHub>("/hub");

app.Run();
