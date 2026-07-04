using System.Collections.Immutable;
using LiteraWorker.Core.Models;

namespace LiteraWorker.Core.Services.Printing;

public interface IPrintOps
{
    /// <summary>
    /// Get printers information regarding configuration support, name, and URI 
    /// </summary>
    /// <param name="token">Token for cancelling operation</param>
    /// <returns></returns>
    ValueTask<IImmutableList<Printer>> GetPrintersInfo(CancellationToken token);

    /// <summary>
    /// Start printing either through CUPS in UNIX or directly in 
    /// Win32 API in Windows
    /// </summary>
    /// <param name="jobs">List of print jobs to print to</param>
    /// <param name="token">Token for cancelling operation</param>
    /// <returns></returns>
    Task Print(IImmutableList<PrintJob> jobs, CancellationToken token);

    Task CancelPrint(PrintJob job, CancellationToken token);
}