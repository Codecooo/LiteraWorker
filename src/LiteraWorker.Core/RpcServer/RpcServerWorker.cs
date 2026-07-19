using System.Net.Sockets;
using LiteraWorker.Core.Services.Auth;
using LiteraWorker.Core.Services.Caching;
using LiteraWorker.Core.Services.Printing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using StreamJsonRpc;

namespace LiteraWorker.Core.RpcServer;

public class RpcServerWorker(ILogger<RpcServerWorker> logger, IServiceProvider sp) : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken ct)
    {
        var transport = sp.GetRequiredService<IRpcTransport>();
        while (!ct.IsCancellationRequested)
        {
            var socket = await transport.AcceptAsync(ct);
            _ = Task.Run(async () =>
            {
                try
                {
                    await HandleConnection(socket, sp);
                }
                catch (Exception ex)
                {
                    logger.LogError(ex, "Error occurred while handling RPC connection");
                }
            }, CancellationToken.None);
        }
    }
    
    public static async Task HandleConnection(Socket socket, IServiceProvider sp)
    {
        using var stream = new NetworkStream(socket, ownsSocket: true);
        using var jsonRpc = new JsonRpc(stream);

        var printOps = sp.GetRequiredService<IPrintOps>();
        var printerCache = sp.GetRequiredService<IPrinterCache>();
        var deviceCache = sp.GetRequiredService<IDeviceCache>();
        var userCache = sp.GetRequiredService<IUserCache>();
        var auth = sp.GetRequiredService<IAuthProvider>();

        jsonRpc.AddLocalRpcTarget(printOps);
        jsonRpc.AddLocalRpcTarget(printerCache);
        jsonRpc.AddLocalRpcTarget(deviceCache);
        jsonRpc.AddLocalRpcTarget(userCache);
        jsonRpc.AddLocalRpcTarget(auth);

        jsonRpc.StartListening();
        await jsonRpc.Completion;
    }
}