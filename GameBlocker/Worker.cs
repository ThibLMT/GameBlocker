using GameBlocker.Models;
using GameBlocker.Services;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace GameBlocker;

public class Worker : BackgroundService
{
    private readonly ILogger<Worker> _logger;
    private readonly IProcessManager _processManager;


    private readonly IOptionsMonitor<AppConfig> _configMonitor;

    // Dependency Injection happens here!
    public Worker(
        ILogger<Worker> logger,
        IProcessManager processManager,
         IOptionsMonitor<AppConfig> configMonitor)
    {
        _logger = logger;
        _processManager = processManager;
        _configMonitor = configMonitor;

        // Log when config changes just in case :)
        _configMonitor.OnChange(newConfig => {
            _logger.LogInformation("Configuration changed! Enabled: {Enabled}, Blocklist: {Count}",
                newConfig.IsEnabled, newConfig.BlockedProcesses.Count);
        });
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("Worker started.");

        while (!stoppingToken.IsCancellationRequested)
        {
            var config = _configMonitor.CurrentValue;

            // 🔍 DEBUGGING: Print the state every loop
            /*_logger.LogInformation("DEBUG CHECK: IsEnabled={Enabled}, Count={Count}, FirstItem={First}",
                config.IsEnabled,
                config.BlockedProcesses?.Count ?? -1,
                config.BlockedProcesses?.FirstOrDefault() ?? "NULL");*/

            if (!config.IsEnabled)
            {
                _logger.LogInformation("Service disabled by config.");
                await Task.Delay(5000, stoppingToken);
                continue;
            }

            var blockList = new HashSet<string>(config.BlockedProcesses ?? new List<string>(), StringComparer.OrdinalIgnoreCase);

            RunCycle(blockList);

            await Task.Delay(5000, stoppingToken);
        }
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