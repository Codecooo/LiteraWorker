namespace LiteraWorker.Core.RpcServer;

public interface IRpcTransport
{
    Task<Stream> AcceptAsync(CancellationToken cancellationToken);
}
