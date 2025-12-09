using GameBlocker;
using GameBlocker.Models;
using GameBlocker.Services;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
using Serilog;

// 1. Setup Logging
Log.Logger = new LoggerConfiguration()
    .WriteTo.Console()
    .WriteTo.File("C:\\ProgramData\\GameBlocker\\log.txt", rollingInterval: RollingInterval.Day)
    .CreateLogger();

var options = new WebApplicationOptions
{
    Args = args,
    ContentRootPath = AppContext.BaseDirectory
};

try
{
    var builder = WebApplication.CreateBuilder(options);

    // 2. Configure Windows Service + Base Path
    builder.Host.UseWindowsService();
    builder.Host.UseSerilog();

    // 3. Register Services
    builder.Services.Configure<AppConfig>(builder.Configuration.GetSection("GameBlocker"));
    builder.Services.AddHostedService<Worker>(); // Background Loop
    builder.Services.AddSingleton<IProcessManager, ProcessManager>();
    builder.Services.AddSingleton<GameStateService>();

    // 4. CORS (Allow React to talk to us)
    builder.Services.AddCors(options =>
    {
        options.AddPolicy("AllowReact", policy =>
            policy.WithOrigins("http://localhost:5173") // React Dev Server Port
                  .AllowAnyMethod()
                  .AllowAnyHeader());
    });

    var app = builder.Build();
    app.UseCors("AllowReact");

    // 5. API Endpoints

    // GET /api/status
    app.MapGet("/api/status", (GameStateService state) =>
    {
        return Results.Ok(new
        {
            isEnabled = state.IsEnabled,
            killCount = state.KillCount,
            recentLogs = state.GetRecentLogs()
        });
    });

    // POST /api/toggle
    app.MapPost("/api/toggle", (GameStateService state) =>
    {
        state.IsEnabled = !state.IsEnabled;

        state.AddLog($"Service toggled to {state.IsEnabled} via Dashboard");

        return Results.Ok(new { message = "State updated", newState = state.IsEnabled });
    });

    // 6. Run on Port 5000
    app.Run("http://localhost:5000");
}
catch (Exception ex)
{
    Log.Fatal(ex, "Service crashed!");
}
finally
{
    Log.CloseAndFlush();
}