using System;
using System.Diagnostics;
namespace GameBlocker
{
    internal class Program
    {
        static void Main(string[] args)
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

                    // The $ allows string interpolation (Like Go's fmt.Printf or JS template literals)
                    Console.WriteLine($"Process: {p.ProcessName} | Title: {p.MainWindowTitle}");
                }
            }

            Console.WriteLine("Enter process name to kill: ");

            string target = Console.ReadLine() ?? "";

            Console.WriteLine($"You entered process : {target}");

            foreach (Process process in userApps)
            {
                if (String.Equals(process.ProcessName, target, StringComparison.OrdinalIgnoreCase))
                {
                    process.Kill();
                    process.WaitForExit();
                }

            }

            // Keep the window open so you can read it (Optional in VS, but good for CLI)
            Console.WriteLine("\nPress any key to exit...");
            Console.ReadKey();
        }
    }
}
