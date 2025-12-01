using GameBlocker.Infrastructure;
using GameBlocker.Models;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Text;
using System.Text.Json;

namespace GameBlocker.Services;

public class ConfigLoader : IConfigLoader
{
    private readonly ILogger<ConfigLoader> _logger;

    // CONSTRUCTOR
    public ConfigLoader(ILogger<ConfigLoader> logger)
    {
        _logger = logger;
    }

    public AppConfig LoadConfig(string filePath)
    {
        try
        {
            _logger.LogInformation("Loading config from {FilePath}", filePath);
            string jsonString = File.ReadAllText(filePath);
            var config = JsonSerializer.Deserialize(jsonString, AppConfigJsonContext.Default.AppConfig);
            return config ?? new AppConfig { BlockedProcesses = new List<string>() };
        }
        catch (Exception ex)
        {
            // 1. Log the error here
            _logger.LogError(ex, "Failed to load config file. Using empty blocklist default.");

            // 2. Return a safe empty object so the app doesn't crash
            return new AppConfig { BlockedProcesses = new List<string>() };
        }
    }
}
