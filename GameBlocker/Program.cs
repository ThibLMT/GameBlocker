using GameBlocker.Models;
using GameBlocker.Services;
using Microsoft.Extensions.Logging;
using System;
using System.Diagnostics;
namespace GameBlocker;

internal class Program
{
    static async Task Main(string[] args)
    {
        using ILoggerFactory loggerFactory = LoggerFactory.Create(builder =>
        {
            builder.AddConsole();
            builder.SetMinimumLevel(LogLevel.Information);
        });

        ILogger<Program> mainLogger = loggerFactory.CreateLogger<Program>();

        mainLogger.LogInformation("GameBlocker is initializing...");

        var processManagerLogger = loggerFactory.CreateLogger<ProcessManager>();
        var manager = new ProcessManager(processManagerLogger);

        var configLoaderLogger = loggerFactory.CreateLogger<ConfigLoader>();
        var configLoader = new ConfigLoader(configLoaderLogger);

        AppConfig config = configLoader.LoadConfig("config.json");

        var blockList = new HashSet<string>(config.BlockedProcesses, StringComparer.OrdinalIgnoreCase);

        mainLogger.LogInformation("Loaded {blockCount} blocked processes.", blockList.Count);

        using var cts = new CancellationTokenSource();

        Console.CancelKeyPress += (sender, eventArgs) =>
        {
            mainLogger.LogInformation("Shutdown signal received. Stopping...");

            // Prevent the process from dying immediately
            eventArgs.Cancel = true;

            // Trigger the token cancellation
            cts.Cancel();
        };

        mainLogger.LogInformation("Monitoring started. Press Ctrl+C to stop.");

        try
        {
            // Change while(true) to check the token
            while (!cts.Token.IsCancellationRequested)
            {
                var userApps = manager.GetUserApps();
                foreach (var app in userApps)
                {
                    // Pass the token here too? (Optional, but good for long ops)
                    if (blockList.Contains(app.ProcessName))
                    {
                        mainLogger.LogInformation("VIOLATION DETECTED: {ProcessName}", app.ProcessName);
                        manager.KillProcessByName(app.ProcessName);
                    }
                }

                mainLogger.LogInformation("...scanning complete. Sleeping.");

                // 4. THE MAGIC: Pass the token to Delay
                // This makes the sleep CANCELABLE immediately.
                // If you press Ctrl+C during the 5s sleep, it wakes up instantly.
                await Task.Delay(5000, cts.Token);
            }
        }
        catch (TaskCanceledException)
        {
            // This is expected behavior when Task.Delay is cancelled.
            // We catch it to exit cleanly.
            mainLogger.LogInformation("Sleep cancelled. Exiting loop.");
        }

        mainLogger.LogInformation("GameBlocker stopped cleanly.");


    }
}
