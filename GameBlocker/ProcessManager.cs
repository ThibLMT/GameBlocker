using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;

namespace GameBlocker
{
    public class ProcessManager
    {
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

        public void KillProcessByName (string targetName)
        {
            foreach (Process process in GetUserApps())
            {
                if (String.Equals(process.ProcessName, targetName, StringComparison.OrdinalIgnoreCase))
                {
                    process.Kill();
                    process.WaitForExit();
                }

            }
        }
    }
}
