using System.Collections.Immutable;
using LiteraWorker.Core.Helpers;
using LiteraWorker.Core.Models;
using PolyType;
using StreamJsonRpc;

namespace LiteraWorker.Core.Services.Caching;

[JsonRpcContract, GenerateShape(IncludeMethods = MethodShapeFlags.PublicInstance)]
public partial interface IDeviceCache
{
    ValueTask<Result<IImmutableList<Device>>> GetAvailableDevices(CancellationToken token);
    
    ValueTask<Result<Device>> GetCurrentDevice(CancellationToken token);
    ValueTask ClearCache(CancellationToken token);
}
