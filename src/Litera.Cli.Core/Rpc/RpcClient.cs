using LiteraWorker.Core.Services.Auth;
using LiteraWorker.Core.Services.Caching;
using LiteraWorker.Core.Services.Printing;
using Microsoft.Extensions.DependencyInjection;
using StreamJsonRpc;

namespace Litera.Cli.Core.Rpc;

public static class RpcClient
{
    public static IServiceCollection AddRpcClient(this IServiceCollection services, Stream stream)
    {
        var jsonRpc = JsonRpc.Attach(stream);

        var printOps = jsonRpc.Attach<IPrintOps>();
        var printerCache = jsonRpc.Attach<IPrinterCache>();
        var deviceCache = jsonRpc.Attach<IDeviceCache>();
        var userCache = jsonRpc.Attach<IUserCache>();
        var auth = jsonRpc.Attach<IAuthProvider>();

        services.AddSingleton(printOps);
        services.AddSingleton(printerCache);
        services.AddSingleton(deviceCache);
        services.AddSingleton(userCache);
        services.AddSingleton(auth);
        return services;
    }
}