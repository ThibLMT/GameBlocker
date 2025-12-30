using GameBlocker;
using GameBlocker.Models;
using GameBlocker.Services;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
using Serilog;
using Serilog.Events;



var options = new WebApplicationOptions
{
    Args = args,
    ContentRootPath = AppContext.BaseDirectory
};

var commonPath = Path.Combine(
        Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData),
        "GameBlocker"
    );

// Define and create the logs subdirectory
var logDirectory = Path.Combine(commonPath, "logs");
Directory.CreateDirectory(logDirectory);
var logFilePath = Path.Combine(logDirectory, "log.txt");

// 1. Setup Logging with the dynamic path
Log.Logger = new LoggerConfiguration()
    .MinimumLevel.Information()
    .MinimumLevel.Override("Microsoft.AspNetCore", LogEventLevel.Warning) // Quiet HTTP logs
    .WriteTo.Console()
    .WriteTo.File(logFilePath, rollingInterval: RollingInterval.Day)
    .CreateLogger();

try
{
    var builder = WebApplication.CreateBuilder(options);

    // 2. Configure Windows Service + Base Path
    builder.Host.UseWindowsService();
    builder.Host.UseSerilog();

    // 3. Register Services
    builder.Services.Configure<AppConfig>(builder.Configuration.GetSection("GameBlocker"));
    builder.Services.Configure<ScannerConfig>(builder.Configuration.GetSection("Scanner"));
    builder.Services.AddHostedService<Worker>(); // Background Loop
    builder.Services.AddSingleton<IProcessManager, ProcessManager>();
    builder.Services.AddSingleton<GameStateService>();
    builder.Services.AddSingleton<UserRulesService>();
    builder.Services.AddTransient<GameScannerService>();

    // 4. CORS (Allow React to talk to us)
    builder.Services.AddCors(options =>
    {
        options.AddPolicy("AllowReact", policy =>
            policy.WithOrigins("http://localhost:5173") // React Dev Server Port
                  .AllowAnyMethod()
                  .AllowAnyHeader());
    });

    var app = builder.Build();

    app.UseSerilogRequestLogging(options =>
    {
        // This template gives you the clean single-line log you want
        options.MessageTemplate = "HTTP {RequestMethod} {RequestPath} responded {StatusCode} in {Elapsed:0.0000} ms";
    });

    app.UseFileServer();
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

    // GET /api/scan
    app.MapPost("/api/scan", (string path, GameScannerService scannerService) =>
    {
        if (string.IsNullOrEmpty(path))
        {
            return Results.BadRequest(error: new { error = "Path cannot be empty" });
        }
        var rawDict = scannerService.ScanGames(path);

        var response = rawDict.Select(kvp => new GameScanResult
        {
            Name = kvp.Key,
            Exes = kvp.Value
        }).ToList();
        return Results.Ok(response);
    });

    // GET /api/blocklist
    app.MapGet("/api/blocklist", (UserRulesService userRulesService) =>
    {
        var rules = userRulesService.LoadRules();
        return Results.Ok(new
        {
            rules
        });
    });

    // POST /api/blocklist
    app.MapPost("/api/blocklist", (List<string> selectedExes, UserRulesService userRulesService) =>
    {
        userRulesService.SaveRules(selectedExes);
        return Results.Ok(new { message = "User  blocked processes updated" });
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