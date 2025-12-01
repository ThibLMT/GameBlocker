using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;

namespace GameBlocker.Services;

public class ProcessManager : IProcessManager
{
    // Field to hold the logger
    private readonly ILogger<ProcessManager> _logger;

    // Constructor
    public ProcessManager(ILogger<ProcessManager> logger)
    {
        _logger = logger;
    }

    public List<ProcessInfo> GetUserApps()
    {
        //List all running processes
        Process[] processList = Process.GetProcesses();

        List<ProcessInfo> userApps = new List<ProcessInfo>();

        // We iterate over the array
        foreach (Process p in processList)
        {

            userApps.Add(new ProcessInfo
            {
                ProcessName = p.ProcessName,
                Id = p.Id
            });

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
