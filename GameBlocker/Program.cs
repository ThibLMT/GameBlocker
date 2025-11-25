using System.Diagnostics;
namespace GameBlocker
{
    internal class Program
    {
        static void Main(string[] args)
        {
            //List all running processes
            Process[] processList = Process.GetProcesses();

            // We iterate over the array
            foreach (Process p in processList)
            {
                // We only print if there is a Window Title (to avoid spamming system services)
                if (!string.IsNullOrEmpty(p.MainWindowTitle))
                {
                    // The $ allows string interpolation (Like Go's fmt.Printf or JS template literals)
                    Console.WriteLine($"Process: {p.ProcessName} | Title: {p.MainWindowTitle}");
                }
            }

            // Keep the window open so you can read it (Optional in VS, but good for CLI)
            Console.WriteLine("\nPress any key to exit...");
            Console.ReadKey();
        }
    }
}
