using LiteraWorker.Core.Services.ApiService;
using LiteraWorker.Core.Services.Auth;
using LiteraWorker.Core.Services.Caching;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace LiteraWorker.Core.Services;

public static class ServiceCollectionExtensions
{
    /// <summary>
    /// Register the core services needed for the console apps to the dependency injection
    /// </summary>
    /// <param name="services">IServiceCollection instance to register the dependency</param>
    /// <returns>IServiceCollection for configuring further the services</returns>
    public static IServiceCollection AddCoreServices(this IServiceCollection services)
    {
        var date = DateTime.UtcNow.ToString("yyy-MM-dd");
        
        services.AddLogging((builder) =>
        {   
            builder.AddFileLogging();
            builder.AddConsole();
        });

        services.AddSingleton<IKeyValueStorage, KeyValueStorage>();
        services.AddSingleton<IPersistentIdentity, PersistentIdentity>();
        services.AddSingleton<ITokensProvider, TokensProvider>();
        services.AddSingleton<ITokenCache, TokenCache>();
        services.AddTransient<AuthHandler>();
        services.AddTransient<GlobalHttpErrorHandler>();

        services.AddHttpClient("AuthClient", client =>
        {
            client.BaseAddress = new Uri("http://localhost:7216");
        });

        services.AddHttpClient("LiteraClient", client =>
        {
            client.BaseAddress = new Uri("http://localhost:7216");
        })
        .AddHttpMessageHandler<AuthHandler>()
        .AddHttpMessageHandler<GlobalHttpErrorHandler>();

        services.AddSingleton<HttpService>();
        services.AddSingleton<UserService>();
        services.AddSingleton<IUserCache, UserCache>();
        services.AddSingleton<DeviceService>();      
        services.AddSingleton<FileService>();
        services.AddSingleton<AuthService>();
        services.AddSingleton<IAuthProvider, AuthProvider>();
        services.AddSingleton<LocalAuthHandler>();
        services.AddSingleton<PrinterService>();
        services.AddSingleton<PrintJobService>();
        services.AddSingleton<IDeviceCache, DeviceCache>();
        services.AddSingleton<IPrinterCache, PrinterCache>();

        return services;
    }

    static IServiceCollection AddFileLogging(this ILoggingBuilder builder)
    {
        var date = DateTime.UtcNow.ToString("yyy-MM-dd");
        var logLocation = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "litera", "worker-logs", $"{date}-worker-logs.txt");

        if (OperatingSystem.IsLinux())
        {
            var logsDir = Environment.GetEnvironmentVariable("LOGS_DIRECTORY") ?? string.Empty;
            logLocation = Path.Combine(logsDir, "worker-logs", $"{date}-logs.txt");
        }

        builder.AddFile(logLocation);
        return builder.Services;
    }
}