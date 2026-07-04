using System.Collections.Immutable;
using LiteraWorker.Core.Models;
using SharpIpp.Models;

namespace LiteraWorker.Core.Services.Printing;

public interface IJobsHandler
{
    /// <summary>
    /// Add print job to the current job entries
    /// </summary>
    /// <param name="printJob">printjob type</param>
    /// <param name="jobResponse">Response of job creation from CUPS</param>
    Task AddJobAsync(PrintJob printJob, PrintJobResponse? jobResponse = default);

    /// <summary>
    /// Update print job status continously
    /// </summary>
    /// <param name="printJob"></param>
    /// <param name="token"></param>
    /// <returns></returns>
    Task UpdateJob(CancellationToken token);

    /// <summary>
    /// Remove print job from the current job entries
    /// </summary>
    /// <param name="printJob"></param>
    void RemoveJob(PrintJob printJob);

    /// <summary>
    /// Get all of running print jobs
    /// </summary>
    /// <param name="ct">Cancellation token to cancel the process</param>
    /// <returns></returns>
    ValueTask<IImmutableList<PrintJob>> GetCurrentJobs(CancellationToken ct);

    /// <summary>
    /// An event for handling print job status updates
    /// </summary>
    event EventHandler PrintJobUpdated;
}