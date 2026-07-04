using System.Collections.Immutable;
using LiteraWorker.Core.Helpers;
using LiteraWorker.Core.Models;

namespace LiteraWorker.Core.Services.Caching;

public interface IPrinterCache
{
    ValueTask<Result<Printer>> GetPrinterDetails(Guid id, CancellationToken token);
    ValueTask<Result<IImmutableList<Printer>>> GetPrinters(CancellationToken token);
    ValueTask ClearCache(CancellationToken token);
}
