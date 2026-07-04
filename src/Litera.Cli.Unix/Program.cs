using Litera.Cli.Core.Commands;
using Litera.Cli.Unix.Commands;
using LiteraWorker.Core.Networks.SignalRClient;
using LiteraWorker.Core.Services;
using LiteraWorker.Core.Services.Caching;
using LiteraWorker.Core.Services.Printing;
using LiteraWorker.Unix.Sevices.Printing;
using Microsoft.Extensions.DependencyInjection;
using XenoAtom.CommandLine;

var services = new ServiceCollection();
services.AddCoreServices();
services.AddSingleton<IJobsHandler, JobsHandlerUnix>();
services.AddSingleton<IPrintOps, PrintOpsUnix>();
services.AddSingleton<IPrinterCache, PrinterCache>();
services.AddSingleton<PrintClient>();
services.AddHostedService<PrintClientWorker>();
services.AddSingleton<StatusCommand>();
services.AddSingleton<DiscoverCommand>();
services.AddSingleton<LoginCommand, LoginCommandUnix>();
services.AddSingleton<RegisterCommand, RegisterCommandUnix>();

var sp = services.BuildServiceProvider();
var loginCommand = sp.GetRequiredService<LoginCommand>();
var registerCommand = sp.GetRequiredService<RegisterCommand>();
var statusCommand = sp.GetRequiredService<StatusCommand>();
var discoverCommand = sp.GetRequiredService<DiscoverCommand>();

var app = new CommandApp("litera")
{
    new CommandUsage(),
    new HelpOption(),
    loginCommand.Build(),
    registerCommand.Build(),
    statusCommand.Build(),
    discoverCommand.Build()
};

await app.RunAsync(args);
