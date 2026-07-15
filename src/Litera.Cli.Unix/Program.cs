using System.Net.Sockets;
using Litera.Cli.Core.Commands;
using Litera.Cli.Core.Rpc;
using Litera.Cli.Unix.Commands;
using Microsoft.Extensions.DependencyInjection;
using XenoAtom.CommandLine;

var services = new ServiceCollection();
services.AddRpcClient(await ConnectAsync());
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

/// <summary>
/// Connects to the RPC server on systemd service socket
/// </summary>
/// <returns></returns>
static async Task<Stream> ConnectAsync()
{
    var socket = new Socket(AddressFamily.Unix, SocketType.Stream, ProtocolType.Unspecified);
    await socket.ConnectAsync(new UnixDomainSocketEndPoint("/run/litera-worker.sock"));

    var stream = new NetworkStream(socket, ownsSocket: true);
    return stream;
}