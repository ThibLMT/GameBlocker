using GameBlocker.Models;
using GameBlocker.Services;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace GameBlocker;

public class Worker : BackgroundService
{
    private readonly ILogger<Worker> _logger;
    private readonly ProcessManager _processManager;
    private readonly ConfigLoader _configLoader;

    // Dependency Injection happens here!
    public Worker(
        ILogger<Worker> logger,
        ProcessManager processManager,
        ConfigLoader configLoader)
    {
        _logger = logger;
        _processManager = processManager;
        _configLoader = configLoader;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        // Setup Phase
        AppConfig config = _configLoader.LoadConfig("config.json");
        var blockList = new HashSet<string>(config.BlockedProcesses, StringComparer.OrdinalIgnoreCase);

        _logger.LogInformation("Worker started. Monitoring {Count} processes.", blockList.Count);

        // The Main Loop
        try
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                var userApps = _processManager.GetUserApps();
                foreach (var app in userApps)
                {
                    if (blockList.Contains(app.ProcessName))
                    {
                        _logger.LogInformation("VIOLATION DETECTED: {ProcessName}", app.ProcessName);
                        _processManager.KillProcessByName(app.ProcessName);
                    }
                }

                // Wait for 5 seconds OR until shutdown is requested
                await Task.Delay(5000, stoppingToken);
            }
        }
        catch (TaskCanceledException)
        {
            // Valid way to exit
        }

        _logger.LogInformation("Worker is stopping.");
    }
}