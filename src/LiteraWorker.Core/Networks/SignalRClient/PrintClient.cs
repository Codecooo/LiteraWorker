using System.Collections.Immutable;
using LiteraWorker.Core.Services.ApiService;
using Microsoft.AspNetCore.SignalR.Client;
using Microsoft.Extensions.Logging;
using LiteraWorker.Core.Models;
using LiteraWorker.Core.Services.Auth;
using LiteraWorker.Core.Services.Printing;
using LiteraWorker.Core.Services.Caching;
using LiteraWorker.Core.Helpers;
using LiteraWorker.Core.DTO;
using LiteraWorker.Core.Mappers;

namespace LiteraWorker.Core.Networks.SignalRClient;

public class PrintClient : IAsyncDisposable
{
    private HubConnection _connection;
    private readonly IDeviceCache _deviceCache;
    private readonly IPrintOps _printOps;
    private readonly FileService _fileService;
    private readonly ITokenCache _tokenCache;
    private readonly ILogger<PrintClient> _logger;
    private bool _initialized;

    public event EventHandler<PrintJobUpdatedEventArgs>? PrintJobUpdatedHub;

    public PrintClient(IDeviceCache deviceCache, IPrintOps printOps, FileService fileService, ITokenCache tokenCache, ILogger<PrintClient> logger)
    {
        _deviceCache = deviceCache;
        _printOps = printOps;
        _fileService = fileService;
        _tokenCache = tokenCache;
        _logger = logger;

        _connection = new HubConnectionBuilder()
            .WithUrl("http://localhost:7216/print")
            .WithAutomaticReconnect()
            .Build();
    }

    /// <summary>
    /// Must be called once after construction to connect and set up subscriptions.
    /// </summary>
    public async Task InitializeAsync(CancellationToken token = default)
    {
        if (_initialized) return;
        _initialized = true;

        Result<Device> deviceResult = new();
        string accessToken;

        try
        {
            deviceResult = await _deviceCache.GetCurrentDevice(token);
            accessToken = await _tokenCache.GetAccessTokenAsync(token);
        }
        catch (Exception ex) when (ex is IdentityNotFoundException || ex is HttpRequestFailedException)
        {
            _logger.LogWarning("No auth tokens or identity found when initializing PrintClient. The client will not connect to the print hub until tokens and identity are present.");
            return;
        }

        // Update the connection URL dynamically
        _connection.HandshakeTimeout = TimeSpan.FromSeconds(15);
        _connection.ServerTimeout = TimeSpan.FromSeconds(30);

        if (!deviceResult.Successful)
        {
            _logger.LogWarning("No current device found when trying to connect to print hub");
            return;
        }

        var newUrl = $"http://localhost:7216/print?device_id={deviceResult.Value!.Id}";
        _connection = new HubConnectionBuilder()
            .WithUrl(newUrl, options =>
            {
                options.AccessTokenProvider = () => Task.FromResult(accessToken);
            })
            .WithAutomaticReconnect()
            .Build();

        await _connection.StartAsync(token);

        _logger.LogInformation("The app is connected with the print hub!");

        SubscribeToPrintRequests(token);
        SubscribeToPrintJobUpdate(token);
    }

    public async Task SendPrintRequest(IImmutableList<SendPrintJobDto> printJobs)
    {
        await _connection.InvokeAsync("SendPrintRequest", printJobs);
    }

    public void SubscribeToPrintRequests(CancellationToken token)
    {
        _connection.On<List<ServerPrintJobDto>>("ReceivePrintRequest", jobs =>
        {            
            _ = Task.Run(async () =>
            {
                try
                {
                    var device = jobs.Select(j => j.DeviceSenderId).FirstOrDefault();
                    _logger.LogInformation("Receiving print request from {deviceId} with {jobs} jobs", device, jobs.Count);

                    var filenames = jobs.Select(j => j.Filename).ToList();
                    var downloaded = await _fileService.DownloadFilesAsync(filenames, token);
                    var printJobs = jobs.Select(j => j.ToPrintJob()).ToImmutableArray();
                    var updated = printJobs.Select(j =>
                    {
                        var path = downloaded.FirstOrDefault(d => d.Contains(j.Filename));
                        return j with { FilePath = path };
                    }).ToImmutableArray();

                    await _printOps.Print(updated, token);
                }
                catch (Exception ex)
                {
                    _logger.LogError("There was an error when receiving print request from hub: {err}", ex.Message);
                }
            });
        });
    }

    public void SubscribeToPrintJobUpdate(CancellationToken token)
    {
        _connection.On<List<ServerPrintJobDto>>("PrintJobStatusUpdated", jobs =>
        {
            _ = Task.Run(async () =>
            {
                try
                {
                    OnPrintJobUpdated(new PrintJobUpdatedEventArgs(jobs.Select(j => j.ToPrintJob()).ToList()));
                }
                catch (Exception ex)
                {
                    _logger.LogError("There was an error when receiving job update from hub: {err}", ex.Message);
                }
            });
        });
    }

    public async Task SendCancelPrintRequest(ServerPrintJobDto printJob)
    {
        await _connection.InvokeAsync("SendCancelPrintRequest", printJob);
    }

    public async Task SendJobStatusUpdate(List<ServerPrintJobDto> printJobs)
    {
        await _connection.InvokeAsync("SendJobStatusUpdate", printJobs);
    }

    protected virtual void OnPrintJobUpdated(PrintJobUpdatedEventArgs e)
    {
        var handler = PrintJobUpdatedHub;
        handler?.Invoke(this, e);
    }

    public async ValueTask DisposeAsync()
    {
        GC.SuppressFinalize(this);
        await _connection.DisposeAsync();
    }
}