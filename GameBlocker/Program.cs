using System;
using System.Diagnostics;
namespace GameBlocker
{
    internal class Program
    {
        static void Main(string[] args)
        {
            var manager = new ProcessManager();

            var userApps = manager.GetUserApps();
            foreach (var app in userApps)
            {
                Console.WriteLine($"Found: {app.ProcessName}");
            }

            Console.WriteLine("Enter process name to kill: ");

            string target = Console.ReadLine() ?? "";

            Console.WriteLine($"You entered process : {target}");

            manager.KillProcessByName(target);

            Console.WriteLine($"Attempted to kill '{target}'.");
            Console.ReadKey();
        }
    }
}
