using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace LiteraWorker.Core.Networks.SignalRClient;

public class PrintClientWorker(PrintClient printClient, ILogger<PrintClientWorker> logger) : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        try
        {
            await printClient.InitializeAsync(stoppingToken);
            await Task.Delay(Timeout.Infinite, stoppingToken);
        }
        catch (Exception ex)
        {
            logger.LogCritical("Litera background worker faced unexpected error and have to shut down: \n {ex}", ex);
            throw;
        }
    }

    public override async Task StopAsync(CancellationToken cancellationToken)
    {
        await printClient.DisposeAsync();
        await base.StopAsync(cancellationToken);
    }
}