using System;
using System.Diagnostics;
namespace GameBlocker
{
    internal class Program
    {
        static async Task Main(string[] args)
        {
            var manager = new ProcessManager();

            var blockList = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
            {
                "notepad",
                "mspaint",
                "ms-teams"
            };

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
}
