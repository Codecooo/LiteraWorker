using LiteraWorker.Core.Services.Auth;
using LiteraWorker.Core.Services.Caching;
using LiteraWorker.Core.Services.Printing;
using Microsoft.Extensions.DependencyInjection;
using StreamJsonRpc;

namespace LiteraWorker.Core.RpcServer;

public class Startup()
{
    public static async Task RegisterRpcServer(IServiceProvider sp)
    {
        var stream = await sp.GetRequiredService<IRpcTransport>().AcceptAsync(CancellationToken.None);
        var jsonRpc = JsonRpc.Attach(stream);

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
    }
}