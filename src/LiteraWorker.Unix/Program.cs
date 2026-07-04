using LiteraWorker.Core.DTO;
using LiteraWorker.Core.Networks.SignalRClient;
using LiteraWorker.Core.RpcServer;
using LiteraWorker.Core.Services;
using LiteraWorker.Core.Services.Caching;
using LiteraWorker.Core.Services.Printing;
using LiteraWorker.Unix.Sevices.Printing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

// var services = new ServiceCollection();

// services.AddCoreServices();
// services.AddSingleton<IJobsHandler, JobsHandlerUnix>();
// services.AddSingleton<IPrintOps, PrintOpsUnix>();
// services.AddSingleton<IPrinterCache, PrinterCache>();
// services.AddSingleton<PrintClient>();
// services.AddHostedService<PrintClientWorker>();
// services.AddSingleton<RpcServerCore>();
// services.AddSingleton<Startup>();

// var sp = services.BuildServiceProvider();
// var s = sp.GetRequiredService<Startup>();
// var c = sp.GetRequiredService<RpcServerCore>();

// await s.RegisterRpcServer();


var host = Host.CreateDefaultBuilder(args)
    .ConfigureServices(services =>
    {
        services.AddCoreServices();
        services.AddSingleton<IJobsHandler, JobsHandlerUnix>();
        services.AddSingleton<IPrintOps, PrintOpsUnix>();
        services.AddSingleton<IPrinterCache, PrinterCache>();
        services.AddSingleton<PrintClient>();
        services.AddHostedService<PrintClientWorker>();
        services.AddSingleton<RpcServerCore>();
        services.AddSingleton<Startup>();
    })
    .ConfigureHostOptions(options =>
    {
        options.BackgroundServiceExceptionBehavior = BackgroundServiceExceptionBehavior.Ignore;
    })
    .Build();

var startup = host.Services.GetRequiredService<Startup>();
_ =  startup.RegisterRpcServer();

await host.RunAsync();
