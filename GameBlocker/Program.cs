using Serilog;
using GameBlocker;
using GameBlocker.Models;
using GameBlocker.Services;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

Log.Logger = new LoggerConfiguration()
    .WriteTo.File("C:\\ProgramData\\GameBlocker\\log.txt", rollingInterval: RollingInterval.Day)
    .CreateLogger();


try
{
    // This sets up the Generic Host (Logging, DI, Config)
    IHost host = Host.CreateDefaultBuilder(args)
    .UseWindowsService()
    .UseContentRoot(AppContext.BaseDirectory)
    .UseSerilog()
    .ConfigureAppConfiguration((context, config) =>
    {
        // Force reloadOnChange: true
        config.AddJsonFile("appsettings.json", optional: false, reloadOnChange: true);
    })
    .ConfigureServices((hostContext, services) =>
    {
        services.Configure<AppConfig>(hostContext.Configuration.GetSection("GameBlocker"));

        services.AddHostedService<Worker>();
        services.AddSingleton<IProcessManager, ProcessManager>();

    })
    .Build();

    // This runs the app and waits for Ctrl+C
    await host.RunAsync();
}
catch (Exception ex)
{
    Log.Fatal(ex, "Service crashed!");
}
finally
{
    Log.CloseAndFlush();
}