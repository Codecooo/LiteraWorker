using LiteraWorker.Core.Networks.SignalRClient;
using LiteraWorker.Core.RpcServer;
using LiteraWorker.Core.Services;
using LiteraWorker.Core.Services.Caching;
using LiteraWorker.Core.Services.Printing;
using LiteraWorker.Unix.Rpc;
using LiteraWorker.Unix.Sevices.Printing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

var host = Host.CreateDefaultBuilder(args)
    .UseSystemd()
    .ConfigureServices(services =>
    {
        services.AddCoreServices();
        services.AddSingleton<IJobsHandler, JobsHandlerUnix>();
        services.AddSingleton<IPrintOps, PrintOpsUnix>();
        services.AddSingleton<IPrinterCache, PrinterCache>();
        services.AddSingleton<IRpcTransport, RpcTransportUnix>();
        services.AddSingleton<PrintClient>();
        services.AddHostedService<PrintClientWorker>();
        services.AddHostedService<RpcServerWorker>();
    })
    .ConfigureHostOptions(options =>
    {
        options.BackgroundServiceExceptionBehavior = BackgroundServiceExceptionBehavior.Ignore;
    })
    .Build();

await host.RunAsync();
