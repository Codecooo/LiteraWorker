using System.Net.Sockets;
using LiteraWorker.Core.RpcServer;

namespace LiteraWorker.Unix.Rpc;

public sealed class RpcTransportUnix : IRpcTransport, IDisposable
{
    private const string SocketPath = "/run/litera-worker.sock";
    private readonly Socket _listener;

    public RpcTransportUnix()
    {
        if (File.Exists(SocketPath))
            File.Delete(SocketPath);

        _listener = new Socket(
            AddressFamily.Unix,
            SocketType.Stream,
            ProtocolType.Unspecified);

        _listener.Bind(new UnixDomainSocketEndPoint(SocketPath));

#pragma warning disable CA1416 // Validate platform compatibility
        File.SetUnixFileMode(
            SocketPath,
            UnixFileMode.UserRead |
            UnixFileMode.UserWrite |
            UnixFileMode.GroupRead |
            UnixFileMode.GroupWrite |
            UnixFileMode.OtherRead |
            UnixFileMode.OtherWrite);
#pragma warning restore CA1416 // Validate platform compatibility

        _listener.Listen();
    }

    public Task<Socket> AcceptAsync(CancellationToken cancellationToken)
        => _listener.AcceptAsync(cancellationToken).AsTask();

    public void Dispose()
    {
        _listener.Dispose();

        if (File.Exists(SocketPath))
            File.Delete(SocketPath);
    }
}