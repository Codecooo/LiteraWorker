using LiteraWorker.Core.Models;

namespace LiteraWorker.Core.Services.Caching;

public sealed class PersistentIdentity(IKeyValueStorage storage) : IPersistentIdentity
{
    private readonly string _filePath = OperatingSystem.IsWindows() ? Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "LiteraWorker", "identity.json")
        : "/var/lib/litera/identity.json";

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
                await storage.SetAsync(_filePath, userIdentity, token);

                return;
            }

            var newIdentity = identity with { UserId = userId == default ? identity.UserId : userId, DeviceId = deviceId == default ? identity.DeviceId : deviceId, ModifiedAt = DateTime.UtcNow };
            await storage.SetAsync(_filePath, newIdentity, token);
        }
        finally
        {
            _lock.Release();
        }
    }

    public async ValueTask<Identity?> LoadIdentity(CancellationToken token) =>
        await storage.GetAsync<Identity>(_filePath, token);

    public async ValueTask ClearIdentity(CancellationToken token) =>
        await storage.ClearAsync(_filePath, token);
}