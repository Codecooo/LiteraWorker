using System.Collections.Concurrent;
using SharpIpp;
using SharpIpp.Models;
using LiteraWorker.Unix.Mappers;
using LiteraWorker.Core.Models;
using System.Collections.Immutable;
using LiteraWorker.Core.Services.Printing;
using LiteraWorker.Core.Models.Enums;
using SharpIpp.Protocol.Models;

namespace LiteraWorker.Unix.Sevices.Printing;

public sealed class JobsHandlerUnix : IJobsHandler
{
    public event EventHandler? PrintJobUpdated;

    private ConcurrentDictionary<int, PrintJob> _currentJobs = new();

    private bool _pollingUpdateRunning = false;

    public Task AddJobAsync(PrintJob printJob, PrintJobResponse jobResponse)
    {
        var processedJob = printJob with
        {
            DeviceJobId = jobResponse.JobId,
            JobStatus = jobResponse.JobState.ToJobStatus(),
            JobMessage = jobResponse.JobStateMessage ?? "Unknown"
        };

        _ = UpdateJob(CancellationToken.None);
        return Task.FromResult(_currentJobs[processedJob.DeviceJobId] = processedJob);
    }

    public ValueTask<IImmutableList<PrintJob>> GetCurrentJobs(CancellationToken cancellationToken)
        => ValueTask.FromResult<IImmutableList<PrintJob>>(_currentJobs.Values.ToImmutableList());

    public void RemoveJob(PrintJob printJob)
        => _currentJobs.TryRemove(printJob.DeviceJobId, out _);

    public async Task UpdateJob(CancellationToken token)
    {
        if (_pollingUpdateRunning) return;
        
        var cupsUri = new Uri(Environment.GetEnvironmentVariable("CUPS_SERVER") ?? "http://localhost:631");

        using var httpClient = new HttpClient();
        using var client = new SharpIppClient(cupsUri, httpClient);

        while (!token.IsCancellationRequested)
        {
            _pollingUpdateRunning = true;
            foreach (var job in _currentJobs.Values.ToList())
            {
                var request = new GetJobAttributesRequest
                {
                    JobId = job.DeviceJobId,
                    PrinterUri = job.Printer.PrinterUri
                };

                var response = await client.GetJobAttributesAsync(request, token);

                var state = response.JobAttributes.JobState ?? JobState.Processing;
                var message = response.JobAttributes.JobStateMessage;

                var updated = job with
                {
                    JobStatus = state.ToJobStatus(),
                    JobMessage = message ?? "Unknown"
                };

                if (updated.JobStatus is JobStatus.Completed
                    or JobStatus.Failed
                    or JobStatus.Canceled)
                {
                    _currentJobs.TryRemove(updated.DeviceJobId, out _);
                }
                else
                {
                    _currentJobs[updated.DeviceJobId] = updated;
                }

                PrintJobUpdated?.Invoke(this, EventArgs.Empty);
            }

            await Task.Delay(2000, token);
        }

        _pollingUpdateRunning = false;
    }

}