using System.Net.Sockets;
using LiteraWorker.Core.RpcServer;

namespace LiteraWorker.Unix.Rpc;

public class RpcTransportUnix : IRpcTransport
{
    public Task<Stream> AcceptAsync(CancellationToken cancellationToken)
    {
        var listener = new Socket(AddressFamily.Unix, SocketType.Stream, ProtocolType.Unspecified);
        listener.Bind(new UnixDomainSocketEndPoint("/run/litera-worker.sock"));
        listener.Listen(1);
        return Task.FromResult<Stream>(new NetworkStream(listener.Accept(), true));
    }
}