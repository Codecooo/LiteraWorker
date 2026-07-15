using System.Net.Sockets;
using LiteraWorker.Core.RpcServer;

namespace LiteraWorker.Unix.Rpc;

public class RpcTransportUnix : IRpcTransport
{
    public async Task<Socket> AcceptAsync(CancellationToken cancellationToken)
    {
        const string SocketPath = "/run/litera-worker.sock";

        if (File.Exists(SocketPath))
            File.Delete(SocketPath);

        var listener = new Socket(AddressFamily.Unix, SocketType.Stream, ProtocolType.Unspecified);

        listener.Bind(new UnixDomainSocketEndPoint(SocketPath));

#pragma warning disable CA1416 // Validate platform compatibility
        File.SetUnixFileMode(
            "/run/litera-worker.sock",
            UnixFileMode.UserRead |
            UnixFileMode.UserWrite |
            UnixFileMode.GroupRead |
            UnixFileMode.GroupWrite |
            UnixFileMode.OtherRead |
            UnixFileMode.OtherWrite);
#pragma warning restore CA1416 // Validate platform compatibility

        listener.Listen();

        return await listener.AcceptAsync(cancellationToken);
    }
}