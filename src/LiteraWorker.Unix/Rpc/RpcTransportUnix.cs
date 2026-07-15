using System.Net.Sockets;
using LiteraWorker.Core.RpcServer;

namespace LiteraWorker.Unix.Rpc;

public class RpcTransportUnix : IRpcTransport
{
    public async Task<Stream> AcceptAsync(CancellationToken cancellationToken)
    {
        const string SocketPath = "/run/litera-worker.sock";

        if (File.Exists(SocketPath))
            File.Delete(SocketPath);

        var listener = new Socket(AddressFamily.Unix, SocketType.Stream, ProtocolType.Unspecified);

        listener.Bind(new UnixDomainSocketEndPoint(SocketPath));
        listener.Listen();

        var socket = await listener.AcceptAsync(cancellationToken);

        return new NetworkStream(socket, ownsSocket: true);
    }
}