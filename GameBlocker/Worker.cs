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

    private readonly GameStateService _state;
    private readonly UserRulesService _userRulesService;


    private readonly IOptionsMonitor<AppConfig> _configMonitor;

    // Dependency Injection happens here!
    public Worker(
        ILogger<Worker> logger,
        IProcessManager processManager,
         IOptionsMonitor<AppConfig> configMonitor,
         GameStateService state,
         UserRulesService userRulesService)
    {
        _logger = logger;
        _processManager = processManager;
        _configMonitor = configMonitor;
        _state = state;

        // Log when config changes just in case :)
        _configMonitor.OnChange(newConfig =>
        {
            _logger.LogInformation("Configuration changed! Enabled: {Enabled}",
                newConfig.IsEnabled);
        });
        _userRulesService = userRulesService;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("Worker started.");

        while (!stoppingToken.IsCancellationRequested)
        {
            var config = _configMonitor.CurrentValue;
            var userRules = _userRulesService.LoadRules();

            // 🔍 DEBUGGING: Print the state every loop
            /*_logger.LogInformation("DEBUG CHECK: IsEnabled={Enabled}, Count={Count}, FirstItem={First}",
                config.IsEnabled */

            if (!_state.IsEnabled)
            {
                await Task.Delay(2000, stoppingToken);
                continue;
            }

            RunCycle(userRules);

            await Task.Delay(5000, stoppingToken);
        }
    }
    public void RunCycle(HashSet<string> currentRules)
    {
        // 1. Fetch all running apps
        var runningApps = _processManager.GetUserApps();

        // 2. Filter and Group by Name (The "Go map" equivalent)
        var violations = runningApps
            .Where(app => currentRules.Contains(app.ProcessName))
            .GroupBy(app => app.ProcessName);

        foreach (var group in violations)
        {
            string processName = group.Key;
            int count = group.Count();

            // 3. Log the summary (One log line instead of many)
            _logger.LogWarning("VIOLATION: Found {ProcessName} ({Count} instances). Terminating...",
                processName, count);

            // 4. Perform the Action
            _processManager.KillProcessByName(processName);

            // 5. Update Shared UI State
            _state.IncrementKillCount();
            _state.AddLog($"Terminated {processName} ({count} instances)");
        }
    }
}