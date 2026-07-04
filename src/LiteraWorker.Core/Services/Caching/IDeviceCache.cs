using System.Collections.Immutable;
using LiteraWorker.Core.Helpers;
using LiteraWorker.Core.Models;

namespace LiteraWorker.Core.Services.Caching;

public interface IDeviceCache
{
    ValueTask<Result<IImmutableList<Device>>> GetAvailableDevices(CancellationToken token);
    
    ValueTask<Result<Device>> GetCurrentDevice(CancellationToken token);
    ValueTask ClearCache(CancellationToken token);
}
