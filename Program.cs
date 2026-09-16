using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using NServiceBus.Logging;

AppContext.SetSwitch("NServiceBus.Core.Hosting.UseV2DeterministicGuid", true);

if (args.Length != 1)
{
    Console.WriteLine("Usage: dotnet run -- [out-of-slot|scoped]");

    return;
}

switch (args[0])
{
    case "out-of-slot":
        await OutOfSlotApplication.InitializeAsync();

        break;
    case "scoped":
        await ScopedApplication.InitializeAsync();

        break;
    default:
        Console.WriteLine("Usage: dotnet run -- [out-of-slot|scoped]");

        break;
}

// This is effectively what your solution is doing at the moment.
// The logging scope is created outside of the host's lifetime scope, which means that services that log before the host starts
// or after the host stops will not have the endpoint scope applied to their log output.
// NServiceBus will fallback to its default logging behavior, which is to log to a text file.
internal static class OutOfSlotApplication
{
    internal static async Task InitializeAsync()
    {
        DemoLogFiles.DeleteExisting();

        using var host = DemoHost.CreateNServiceBusEndpoint("OutOfSlotDemo");

        DemoComponent.LogStartup();
        await host.StartAsync();
        await host.StopAsync();
        DemoComponent.LogShutdown();
        DemoLogFiles.PrintExisting();
    }
}

// Here, the logging scope is created inside of the host's lifetime scope, which means that services that log before the host starts
// or after the host stops will have the endpoint scope applied to their log output. NServiceBus will find the existing logger and
// won't fallback to its default logging behavior. No text file will be created.
internal static class ScopedApplication
{
    internal static async Task InitializeAsync()
    {
        DemoLogFiles.DeleteExisting();

        var host = DemoHost.CreateNServiceBusEndpoint("ScopedDemo");
        var loggerFactory = host.Services.GetRequiredService<Microsoft.Extensions.Logging.ILoggerFactory>();
        var endpointLoggingScope = host.Services.GetRequiredService<EndpointLoggingScope>();
        var logger = loggerFactory.CreateLogger("Demo");

        using (logger.BeginEndpointScope(endpointLoggingScope))
        {
            DemoComponent.LogStartup();
            await host.StartAsync();
            await host.StopAsync();
            DemoComponent.LogShutdown();

            DemoLogFiles.PrintExisting();
        }
    }
}

internal static class DemoHost
{
    internal static IHost CreateNServiceBusEndpoint(string endpointName)
    {
        var builder = Host.CreateApplicationBuilder();
        var endpointConfiguration = new EndpointConfiguration(endpointName);

        endpointConfiguration.UseTransport<LearningTransport>();
        endpointConfiguration.UseSerialization<SystemJsonSerializer>();

        builder.Services.AddNServiceBusEndpoint(endpointConfiguration);

        builder.Logging.ClearProviders();
        builder.Logging.AddSimpleConsole(options => options.SingleLine = true);

        return builder.Build();
    }
}

internal static class DemoLogFiles
{
    internal static void DeleteExisting()
    {
        foreach (var path in Directory.EnumerateFiles(
                     AppContext.BaseDirectory,
                     "nsb_log_*.txt"))
        {
            File.Delete(path);
        }
    }

    internal static void PrintExisting()
    {
        var found = false;
        Console.WriteLine();

        foreach (var path in Directory.EnumerateFiles(
                     AppContext.BaseDirectory,
                     "nsb_log_*.txt"))
        {
            found = true;
            Console.WriteLine($"--- {Path.GetFileName(path)} ---");
            Console.WriteLine(File.ReadAllText(path));
        }

        if (!found)
        {
            Console.WriteLine("No nsb_log_*.txt file was found.");
        }
    }
}

internal static class DemoComponent
{
    // We created a static logger for this demo component to keep this example simple.
    // In a real application, you would use dependency injection to provide an ILogger<T> instance to your components.
    private static readonly ILog Logger = LogManager.GetLogger(nameof(DemoComponent));

    internal static void LogStartup() =>
        Logger.Info("******** Callback queue found. AutoDeleteOnIdle set to 7 days. ********");

    internal static void LogShutdown() =>
        Logger.Info("******** Blob storage data bus stopped. ********");
}
