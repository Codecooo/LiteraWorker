using System.Collections.Immutable;
using FlutterSharpRpc.Services;
using LiteraWorker.Core.DTO;
using LiteraWorker.Core.Helpers;
using LiteraWorker.Core.Mappers;
using LiteraWorker.Core.Models;
using LiteraWorker.Core.Models.Enums;
using LiteraWorker.Core.Networks.SignalRClient;
using LiteraWorker.Core.Services.ApiService;
using LiteraWorker.Core.Services.Auth;
using LiteraWorker.Core.Services.Caching;
using LiteraWorker.Core.Services.Printing;
using Microsoft.Extensions.Logging;

namespace LiteraWorker.Core.RpcServer;

public class RpcServerCore : RpcNotifier, IDisposable
{
    private readonly IPrintOps _printOps;
    private readonly PrintClient _printClient;
    private readonly IJobsHandler _jobsHandler;
    private readonly IPrinterCache _printerCache;
    private readonly IDeviceCache _deviceCache;
    private readonly IUserCache _userCache;
    private readonly IAuthProvider _authProvider;
    private readonly PrinterService _printerService;
    private readonly DeviceService _deviceService;
    private readonly PrintJobService _printJobService;
    private readonly FileService _fileService;
    private readonly ILogger<RpcServerCore> _logger;
    private readonly LocalAuthHandler _localAuthHandler;

    public RpcServerCore(
        IPrintOps printOps,
        PrintClient printClient,
        IJobsHandler jobsHandler,
        IPrinterCache printerCache,
        IDeviceCache deviceCache,
        IUserCache userCache,
        IAuthProvider authProvider,
        PrinterService printerService,
        ILogger<RpcServerCore> logger,
        DeviceService deviceService,
        PrintJobService printJobService,
        FileService fileService,
        LocalAuthHandler localAuthHandler)
    {
        _printOps = printOps;
        _printClient = printClient;
        _jobsHandler = jobsHandler;
        _printerCache = printerCache;
        _deviceCache = deviceCache;
        _userCache = userCache;
        _authProvider = authProvider;
        _printerService = printerService;
        _logger = logger;
        _deviceService = deviceService;
        _printJobService = printJobService;
        _fileService = fileService;
        _localAuthHandler = localAuthHandler;

        _jobsHandler.PrintJobUpdated += async (o, e) => await UpdatePrintJob(o, e);
        _printClient.PrintJobUpdatedHub += async (o, e) => await UpdatePrintJob(o, e);
    }

    public async Task SendPrintRequest(IImmutableList<SendPrintJobDto> sendPrintJobDtos)
    {
        if (sendPrintJobDtos.First().Route == PrintRoute.Local && !OperatingSystem.IsAndroid() && !OperatingSystem.IsIOS())
        {
            var printJobs = sendPrintJobDtos.Select(dto => dto.ToPrintJob()).ToList();

            _logger.LogInformation("Printing locally due to local print route chosen with {count} total jobs", printJobs.Count);
            await _printOps.Print(printJobs.ToImmutableArray(), CancellationToken.None);
            _logger.LogInformation("Printing locally finished");

            await _printJobService.AddPrintJob(printJobs.ToImmutableArray());
            return;
        }

        _logger.LogInformation("Uploading files to the server.. ({jobs} total)", sendPrintJobDtos.Count);
        await _fileService.UploadFilesAsync(sendPrintJobDtos.Select(s => s.FilePath).ToList());
        
        _logger.LogInformation("Sending print request to the print hub with {count} total jobs", sendPrintJobDtos.Count);
        await _printClient.SendPrintRequest(sendPrintJobDtos);
        _logger.LogInformation("Finished sending print request. Wait for response from the other side...");
    }

    public async Task SendCancelPrintRequest(ServerPrintJobDto printJob)
    {
        if (printJob.Route == PrintRoute.Local && !OperatingSystem.IsAndroid() && !OperatingSystem.IsIOS())
        {
            _logger.LogInformation("Cancelling local print for {filename}", printJob.Filename);
            await _printOps.CancelPrint(printJob.ToPrintJob(), CancellationToken.None);
            _logger.LogInformation("Local print for {filename} cancelled", printJob.Filename);
            return;
        }

        _logger.LogInformation("Sending print cancellation request to the print hub for job {filename}", printJob.Filename);
        await _printClient.SendCancelPrintRequest(printJob);
        _logger.LogInformation("Finished sending cancellation print request to the print hub for {filename}", printJob.Filename);
    }

    public async Task NotifyPrintJobUpdateAsync(IImmutableList<PrintJob>? currentJobs)
    {
        await JsonRpc.NotifyAsync("printJobUpdated", currentJobs?.Select(j => j.ToServerPrintJobDto()).ToList());
    }

    public async Task UpdatePrintJob(object? sender, EventArgs eventArgs)
    {
        var currentJobs = await _jobsHandler.GetCurrentJobs(CancellationToken.None);
        _logger.LogInformation("Sending print job update notification to the client");

        if (await NetworkHelpers.InternetAvailable())
        {
            await _printClient.SendJobStatusUpdate(currentJobs.Select(j => j.ToServerPrintJobDto()).ToList());
        }

        await NotifyPrintJobUpdateAsync(currentJobs);
    }

    public async Task<Result<IImmutableList<SendPrinterDto>>> FetchPrinters()
    {
        var result = await _printerCache.GetPrinters(CancellationToken.None);

        if (!result.Successful)
        {
            return Result<IImmutableList<SendPrinterDto>>.Failure(result.Message, result.StatusCode, result.Problem);
        }

        if (result.Value == null || !result.Value.Any())
        {
            return Result<IImmutableList<SendPrinterDto>>.Success(ImmutableArray<SendPrinterDto>.Empty);
        }
        _logger.LogInformation("Fetched printers from the source...");

        var printers = result.Value!.Select(p => p.ToSendPrinterDto()).ToImmutableList();

        return Result<IImmutableList<SendPrinterDto>>.Success(printers);
    }

    public async Task<Result<IImmutableList<Device>>> FetchDevices()
    {
        var devicesResult = await _deviceCache.GetAvailableDevices(CancellationToken.None);
        _logger.LogInformation("Fetched devices from the source...");
        return devicesResult;
    }

    public async Task SaveUser(string password)
    {
        await _localAuthHandler.SaveUser(password);
        _logger.LogInformation("User saved successfully with the provided password.");
    }

    public async Task<bool> IsUserSaved()
    {
        return await _localAuthHandler.IsUserSaved();
    }

    public async Task<Result<EmptyRecord>> DeleteDevice(Guid deviceId)
    {
        _logger.LogInformation("Deleting device for {id}", deviceId);
        await _deviceCache.ClearCache(CancellationToken.None);
        return await _deviceService.DeleteDevice(deviceId, CancellationToken.None);
    }

    public async Task<Result<EmptyRecord>> DeletePrinter(Guid printerId)
    {
        _logger.LogInformation("Deleting printer for {id}", printerId);
        return await _printerService.DeletePrinter(printerId, CancellationToken.None);
    }

    public async Task<IImmutableList<PrintJob>> FetchPrintJobsLocal()
    {
        var currentJobs = await _jobsHandler.GetCurrentJobs(CancellationToken.None);
        _logger.LogInformation("Fetched print jobs from the source...");
        return currentJobs;
    }

    public async Task<Result<IImmutableList<ServerPrintJobDto>>> FetchPrintJobsRemote(Guid userId)
    {
        var currentJobs = await _printJobService.GetPrintJobs(userId);
        _logger.LogInformation("Fetched print jobs from remote");
        return currentJobs;
    }

    public async Task<Result<User>> FetchUser()
    {
        _logger.LogInformation("Fetched user from the source...");
        return await _userCache.GetCurrentUser(CancellationToken.None);
    }

    public async Task<Result<Device>> FetchCurrentDevice()
    {
        _logger.LogInformation("Fetched current device from the source...");
        return await _deviceCache.GetCurrentDevice(CancellationToken.None);
    }

    public async Task<Result<EmptyRecord>> LoginUser(LoginRequestDto user)
    {
        var result = await _authProvider.Login(user);
        _logger.LogInformation("Successfully logged in the user!");
        return result;
    }

    public async Task<Result<EmptyRecord>> LogoutUser()
    {
        var identity = await _userCache.GetCurrentUser(CancellationToken.None);
        _logger.LogInformation(identity!.ToString());
        if (identity.Value is null)
        {
            _logger.LogError("No current user found during logout operation");
            return Result<EmptyRecord>.Failure("No current user found", 404, identity.Problem);
        }

        var result = await _authProvider.Logout(identity.Value.Id);

        _logger.LogInformation("Logout operation completed with success: {success}, reason: {msg}", result.Successful, result.Message);
        return result;
    }

    public async Task<Result<EmptyRecord>> RegisterUser(UserDto userDto)
    {
        var result = await _authProvider.RegisterUser(userDto);
        _logger.LogInformation("Register operation completed with success: {success}, reason: {msg}", result.Successful, result.Message);
        return result;
    }

    public async Task<bool> IsAuthenticated()
    {
        return await _authProvider.IsAuthenticated();
    }

    public async Task UpdatePrinter(Printer printer)
    {
        await _printerService.UpdatePrinter(printer);
        _logger.LogInformation("Successfully updated the printer!");
    }

    public void Dispose()
    {
        GC.SuppressFinalize(this);
        _jobsHandler.PrintJobUpdated -= async (o, e) => await UpdatePrintJob(o, e);
        _printClient.PrintJobUpdatedHub -= async (o, e) => await UpdatePrintJob(o,e);
    }
}