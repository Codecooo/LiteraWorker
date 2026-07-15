using System.Net.Sockets;

namespace LiteraWorker.Core.RpcServer;

public interface IRpcTransport
{
    Task<Socket> AcceptAsync(CancellationToken cancellationToken);
}
