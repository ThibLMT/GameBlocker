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
    private readonly IProcessManager _processManager;
    private readonly IConfigLoader _configLoader;

    // Dependency Injection happens here!
    public Worker(
        ILogger<Worker> logger,
        IProcessManager processManager,
        IConfigLoader configLoader)
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

        try
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                // We call the logic method
                RunCycle(blockList);

                await Task.Delay(5000, stoppingToken);
            }
        }
        catch (TaskCanceledException)
        {
            // Graceful shutdown
        }

        _logger.LogInformation("Worker is stopping.");
    }
    public void RunCycle(HashSet<string> blockList)
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
    }
}