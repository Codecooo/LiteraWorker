using System.Collections.Immutable;
using System.Runtime.InteropServices;
using FlutterSharpRpc.Services;
using LiteraWorker.Core.DTO;
using LiteraWorker.Core.Helpers;
using LiteraWorker.Core.Mappers;
using LiteraWorker.Core.Models;
using LiteraWorker.Core.Models.Enums;
using LiteraWorker.Core.Services.Caching;

namespace LiteraWorker.Core.Services.ApiService;

public class DeviceService(
    HttpService httpService,
    IPersistentIdentity persistentIdentity)
{
    private async Task<CreateDeviceDto> CreateDevice(Guid userId, CancellationToken token)
    {
        return new CreateDeviceDto
        {
            Name = OperatingSystem.IsLinux() ? GetMachineNameLinux() : Environment.MachineName,
            OS = RuntimeInformation.OSDescription,
            DeviceStatus = DevicesStatus.Online,
            IpAddress = NetworkHelpers.GetLocalIpV4(),
            UserId = userId,
            AllowedPeerToPeer = true,
            AllowedToInternet = true,
            CanSend = true,
            CanReceive = !OperatingSystem.IsAndroid() || !OperatingSystem.IsIOS(),
        };
    }

    /// <summary>
    /// Register current device into the API 
    /// </summary>
    /// <param name="token">Cancellation token to cancel request</param>
    /// <returns>Awaitable task for registering device</returns>
    public async ValueTask<Result<Device>> RegisterDevice(Guid userId, CancellationToken token)
    {
        var device = await CreateDevice(userId, token);

        var result = await httpService.PostAsync("api/devices/create", device, JsonContext.Default.DeviceDto, token);

        if (result.Value == null || !result.Successful)
        { 
            return Result<Device>.Failure(result.Message ?? "Failed to register device", result.StatusCode); 
        }

        RpcLog.Info($"Registered new device with id {result.Value!.Id} for user {userId}");
        await persistentIdentity.SaveIdentity(userId, result.Value!.Id, token);
        return Result<Device>.Success(result.Value!.ToDevice());
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="userId"></param>
    /// <param name="token"></param>
    /// <returns></returns>
    public async ValueTask<Result<IImmutableList<Device>>> GetConnectedDevices(Guid userId, CancellationToken token)
    {
        var result = await httpService.GetAsync($"api/devices/available/{userId}", JsonContext.Default.IImmutableListDeviceDto, token);

        if (result.Value == null || !result.Successful)
        { 
            return Result<IImmutableList<Device>>.Failure(result.Message ?? "Failed to get devices", result.StatusCode); 
        }
        
        var devices = result.Value!.ToDeviceList().ToImmutableArray();

        return Result<IImmutableList<Device>>.Success(devices);
    }

    public async ValueTask<Result<EmptyRecord>> DeleteDevice(Guid deviceId, CancellationToken token)
    {
        var result = await httpService.DeleteAsync($"api/devices/delete/{deviceId}", token);

        if (!result.Successful)
        {
            Result<EmptyRecord>.Failure(result.Message ?? "Failed to delete device", result.StatusCode);
        }

        return Result<EmptyRecord>.Success(EmptyRecord.Empty);
    }

    public async ValueTask<Result<Device>> GetCurrentDevice(Guid deviceId, CancellationToken token)
    {
        var query = $"deviceId={deviceId}";
        var result = await httpService.GetAsync($"api/devices?{query}", JsonContext.Default.DeviceDto, token);

        if (result.Value == null || !result.Successful)
        { 
            return Result<Device>.Failure(result.Message ?? "Failed to get device", result.StatusCode); 
        }

        return Result<Device>.Success(result.Value!.ToDevice());
    }

    /// <summary>
    /// Get machine name in linux since Environment.MachineName
    /// is not guaranteed to return the actual machine name
    /// </summary>
    /// <returns></returns>
    private static string GetMachineNameLinux()
    {
        const string boardInfoPath = "/sys/devices/virtual/dmi/id/board_name";
        return File.ReadAllText(boardInfoPath).TrimEnd();
    }
}
