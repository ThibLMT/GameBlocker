using GameBlocker;
using GameBlocker.Models;
using GameBlocker.Services;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

// This sets up the Generic Host (Logging, DI, Config)
IHost host = Host.CreateDefaultBuilder(args)
    .ConfigureServices((hostContext, services) =>
    {
        // 2. The Options Pattern!
        // This says: "Bind the class AppConfig to the 'GameBlocker' section in JSON"
        services.Configure<AppConfig>(hostContext.Configuration.GetSection("GameBlocker"));

        services.AddHostedService<Worker>();
        services.AddSingleton<IProcessManager, ProcessManager>();

        // 3. REMOVE ConfigLoader! We don't need it anymore.
        // services.AddSingleton<IConfigLoader, ConfigLoader>(); <-- DELETE
    })
    .Build();

// This runs the app and waits for Ctrl+C
await host.RunAsync();