using LiteraWorker.Core.Models;

namespace LiteraWorker.Core.Services.Caching;

public interface IPersistentIdentity
{
    ValueTask SaveIdentity(Guid userId = default, Guid deviceId = default, CancellationToken token = default);
    ValueTask<Identity?> LoadIdentity(CancellationToken token);
    ValueTask ClearIdentity(CancellationToken token);
}