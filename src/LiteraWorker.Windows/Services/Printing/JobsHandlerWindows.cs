using System.Collections.Concurrent;
using System.Collections.Immutable;
using LiteraWorker.Core.Models;
using LiteraWorker.Core.Models.Enums;
using LiteraWorker.Core.Services.Printing;
using LiteraWorker.Windows.Mappers;
using SharpIpp.Models;

namespace LiteraWorker.Windows.Services.Printing;

public class JobsHandlerWindows(InteropPrintWindowsNative printWindowsNative) : IJobsHandler
{
    private ConcurrentDictionary<int, PrintJob> _currentJobs = new();

    public event EventHandler? PrintJobUpdated;

    public async Task AddJobAsync(PrintJob printJob, PrintJobResponse? jobResponse)
    {
        var nativeJobs = await printWindowsNative.GetPrintJobs();
        var printJobs = nativeJobs.ToImmutablePrintJobs([.. _currentJobs.Values]);
        var addedJob = printJobs.FirstOrDefault(pj => pj.DeviceJobId == printJob.DeviceJobId);

        _currentJobs[addedJob.DeviceJobId] = printJob;
    }

    public async ValueTask<IImmutableList<PrintJob>> GetCurrentJobs(CancellationToken ct)
    {
        return _currentJobs.Values.ToImmutableList();
    }

    public void RemoveJob(PrintJob printJob)
    {
        _currentJobs.TryRemove(printJob.DeviceJobId, out _);
    }

    public async Task UpdateJob(CancellationToken token)
    {
        var currentJobs = _currentJobs.Values;

        while (currentJobs.Any())
        {
            var nativeJobs = await printWindowsNative.GetPrintJobs();
            var printJobs = nativeJobs.ToImmutablePrintJobs([.. _currentJobs.Values]);

            foreach (var printJob in printJobs)
            {
                _currentJobs[printJob.DeviceJobId] = printJob;
                PrintJobUpdated?.Invoke(this, EventArgs.Empty);

                if (printJob.JobStatus == JobStatus.Completed || printJob.JobStatus == JobStatus.Failed || printJob.JobStatus == JobStatus.Canceled)
                {
                    _currentJobs.TryRemove(printJob.DeviceJobId, out _);
                    currentJobs.Remove(printJob);
                }
            }
        }
    }
}