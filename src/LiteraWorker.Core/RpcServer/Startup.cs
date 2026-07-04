using FlutterSharpRpc;
using LiteraWorker.Core.Helpers;

namespace LiteraWorker.Core.RpcServer;

public class Startup(RpcServerCore core)
{
    public async Task RegisterRpcServer()
    {
        await CsharpRpcServer.StartWithExplicitAsync(core, JsonContext.Default,
        async (rpc, server) =>
        {
            rpc.AddLocalRpcMethod("SendPrintRequest", server.SendPrintRequest);
            rpc.AddLocalRpcMethod("SendCancelPrintRequest", server.SendCancelPrintRequest);
            rpc.AddLocalRpcMethod("FetchPrinters", server.FetchPrinters);
            rpc.AddLocalRpcMethod("UpdatePrinter", server.UpdatePrinter);
            rpc.AddLocalRpcMethod("FetchDevices", server.FetchDevices);
            rpc.AddLocalRpcMethod("FetchPrintJobsLocal", server.FetchPrintJobsLocal);
            rpc.AddLocalRpcMethod("FetchPrintJobsRemote", server.FetchPrintJobsRemote);
            rpc.AddLocalRpcMethod("FetchUser", server.FetchUser);
            rpc.AddLocalRpcMethod("FetchCurrentDevice", server.FetchCurrentDevice);
            rpc.AddLocalRpcMethod("LoginUser", server.LoginUser);
            rpc.AddLocalRpcMethod("LogoutUser", server.LogoutUser);
            rpc.AddLocalRpcMethod("RegisterUser", server.RegisterUser);
            rpc.AddLocalRpcMethod("IsAuthenticated", server.IsAuthenticated);
            rpc.AddLocalRpcMethod("IsUserSaved", server.IsUserSaved);
            rpc.AddLocalRpcMethod("SaveUser", server.SaveUser);
            rpc.AddLocalRpcMethod("DeleteDevice", server.DeleteDevice);
            rpc.AddLocalRpcMethod("DeletePrinter", server.DeletePrinter);
        });
    }
}