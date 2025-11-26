using GameBlocker.Models;
using GameBlocker.Services;
using System;
using System.Diagnostics;
namespace GameBlocker;

internal class Program
{
    static async Task Main(string[] args)
    {
        var manager = new ProcessManager();
        var configLoader = new ConfigLoader();

        AppConfig config;
        try
        {
            config = configLoader.LoadConfig("config.json");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"ERROR: Failed to load config: {ex.Message}");
            Console.WriteLine("Using empty blocklist.");
            config = new AppConfig { BlockedProcesses = new List<string>() };
        }

        var blockList = new HashSet<string>(config.BlockedProcesses, StringComparer.OrdinalIgnoreCase);

        Console.WriteLine($"Loaded {blockList.Count} blocked processes.");

        Console.WriteLine("GameBlocker is running... Monitoring processes.");

        while (true)
        {
            var userApps = manager.GetUserApps();
            foreach (var app in userApps)
            {
                if (blockList.Contains(app.ProcessName))
                {
                    Console.WriteLine($"VIOLATION DETECTED: {app.ProcessName}");
                    manager.KillProcessByName(app.ProcessName);
                }
                
            }

            
            Console.WriteLine("...scanning complete. Sleeping.");
            await Task.Delay(5000); // Pauses for 5 seconds non-blockingly
        }


    }
}
