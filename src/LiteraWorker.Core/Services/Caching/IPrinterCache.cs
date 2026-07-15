using System.Collections.Immutable;
using LiteraWorker.Core.Helpers;
using LiteraWorker.Core.Models;
using PolyType;
using StreamJsonRpc;

namespace LiteraWorker.Core.Services.Caching;

[JsonRpcContract, GenerateShape(IncludeMethods = MethodShapeFlags.PublicInstance)]
public partial interface IPrinterCache
{
    ValueTask<Result<Printer>> GetPrinterDetails(Guid id, CancellationToken token);
    ValueTask<Result<IImmutableList<Printer>>> GetPrinters(CancellationToken token);
    ValueTask ClearCache(CancellationToken token);
}
