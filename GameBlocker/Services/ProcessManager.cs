using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;

namespace GameBlocker.Services;

public class ProcessManager
{
    // Field to hold the logger
    private readonly ILogger<ProcessManager> _logger;

    // Constructor
    public ProcessManager(ILogger<ProcessManager> logger)
    {
        _logger = logger;
    }

    public List<Process> GetUserApps()
    {
        //List all running processes
        Process[] processList = Process.GetProcesses();

        List<Process> userApps = new List<Process>();

        // We iterate over the array
        foreach (Process p in processList)
        {
            // We only print if there is a Window Title (to avoid spamming system services)
            if (!string.IsNullOrEmpty(p.MainWindowTitle))
            {
                userApps.Add(p);

            }
        }

        return userApps;
    }

    public void KillProcessByName(string targetName)
    {
        // We look for the process again to be sure we have the latest handle
        var processes = Process.GetProcessesByName(targetName);

        foreach (var p in processes)
        {
            try
            {
                // LOGGING: Replaces Console.WriteLine
                _logger.LogWarning("Killing prohibited process: {ProcessName} (ID: {Pid})", targetName, p.Id);

                p.Kill();
                p.WaitForExit();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to kill process {ProcessName}", targetName);
            }
        }
    }
}
