using GameBlocker;
using GameBlocker.Services;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

// This sets up the Generic Host (Logging, DI, Config)
IHost host = Host.CreateDefaultBuilder(args)
    .ConfigureServices(services =>
    {
        // 1. Register the Worker
        services.AddHostedService<Worker>();

        // 2. Register your classes as "Singletons"
        // (Created once, lived forever)
        services.AddSingleton<ProcessManager>();
        services.AddSingleton<ConfigLoader>();
    })
    .Build();

// This runs the app and waits for Ctrl+C
await host.RunAsync();