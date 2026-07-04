using LiteraWorker.Core.Models;

namespace LiteraWorker.Core.Services.Caching;

public sealed class PersistentIdentity(IKeyValueStorage storage) : IPersistentIdentity
{
    private const string Key = "litera-identity";

    private static readonly SemaphoreSlim _lock = new(1, 1);

    public async ValueTask SaveIdentity(Guid userId = default, Guid deviceId = default, CancellationToken token = default)
    {
        await _lock.WaitAsync(token);

        try
        {
            var identity = await LoadIdentity(token);

            if (identity is null)
            {
                var userIdentity = new Identity
                {
                    UserId = userId == default ? Guid.Empty : userId,
                    DeviceId = deviceId == default ? Guid.Empty : deviceId,
                    CreatedAt = DateTime.UtcNow,
                    ModifiedAt = DateTime.UtcNow,
                    Fingerprint = Guid.CreateVersion7()
                };
                await storage.SetAsync(Key, userIdentity, token);

                return;
            }

            var newIdentity = identity with { UserId = userId == default ? identity.UserId : userId, DeviceId = deviceId == default ? identity.DeviceId : deviceId, ModifiedAt = DateTime.UtcNow };
            await storage.SetAsync(Key, newIdentity, token);
        }
        finally
        {
            _lock.Release();
        }
    }

    public async ValueTask<Identity?> LoadIdentity(CancellationToken token) =>
        await storage.GetAsync<Identity>(Key, token);

    public async ValueTask ClearIdentity(CancellationToken token) =>
        await storage.ClearAsync(Key, token);
}