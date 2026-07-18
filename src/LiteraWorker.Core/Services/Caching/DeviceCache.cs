using System.Collections.Immutable;
using System.Net;
using LiteraWorker.Core.Services.ApiService;
using LiteraWorker.Core.Helpers;
using LiteraWorker.Core.Models;
using Microsoft.Extensions.Logging;

namespace LiteraWorker.Core.Services.Caching;

public sealed class DeviceCache(
    IUserCache userCache,
    ILogger<DeviceCache> logger,
    DeviceService deviceService,
    IPersistentIdentity persistentIdentity) : IDeviceCache
{
    private readonly static string _cacheDir = Path.Combine(Environment.GetEnvironmentVariable("CACHE_DIRECTORY") ?? "");
    private readonly string _allDevicesCachePath = LocalCacheSet.InitializeCachePath(Path.Combine(_cacheDir, "devices.json"));
    private readonly string _currentDeviceCachePath = LocalCacheSet.InitializeCachePath(Path.Combine(_cacheDir, "current-device.json"));

    public async ValueTask<Result<IImmutableList<Device>>> GetAvailableDevices(CancellationToken token)
    {
        var device = await GetCachedDevice(token, reqCurrentDevice: false);
        if (device != null)
        {
            return Result<IImmutableList<Device>>.Success(device);
        }

        if (!await NetworkHelpers.InternetAvailable())
        {
            logger.LogWarning("App is offline and cannot connect to the API.");
            throw new WebException("No internet connection", WebExceptionStatus.ConnectFailure);
        }

        var userResult = await userCache.GetCurrentUser(token);
        if (!userResult.Successful)
        {
            return Result<IImmutableList<Device>>.Failure(userResult.Message, userResult.StatusCode, userResult.Problem);
        }

        var devicesResult = await deviceService.GetConnectedDevices(userResult.Value!.Id, token);
        if (!devicesResult.Successful)
        {
            return devicesResult;
        }

        await LocalCacheSet.WriteCache(devicesResult.Value!, _allDevicesCachePath);
        return devicesResult;
    }

    public async ValueTask<Result<Device>> GetCurrentDevice(CancellationToken token)
    {
        var device = await GetCachedDevice(token, reqCurrentDevice: true );
        if (device != null)
        {
            return Result<Device>.Success(device!.First());
        }

        var identity = await persistentIdentity.LoadIdentity(token);
        if (!await NetworkHelpers.InternetAvailable())
        {
            logger.LogWarning("App is offline and cannot connect to the API.");
            throw new WebException("No internet connection", WebExceptionStatus.ConnectFailure);
        }
        if (identity is null)
        {
            logger.LogWarning("No persistent identity could be found, communicating with API will be difficult. Relogin back again is recommended");
            throw new IdentityNotFoundException();
        }

        var result = await deviceService.GetCurrentDevice(identity.DeviceId, token);
        if (!result.Successful)
        {
            return result;
        }

        var currentList = ImmutableArray.Create(result.Value!);

        await LocalCacheSet.WriteCache(currentList, _currentDeviceCachePath);
        return result;
    }


    private async Task<IImmutableList<Device>?> GetCachedDevice(
        CancellationToken token,
        bool reqCurrentDevice)
    {
        var path = reqCurrentDevice ? _currentDeviceCachePath : _allDevicesCachePath;

        // if current device requested set validation to one day
        if (reqCurrentDevice && await LocalCacheSet.IsCacheValid(path, TimeSpan.FromDays(1)))
        {
            return await LocalCacheSet.ReadCache<IImmutableList<Device>>(path);
        }

        // If offline, reuse cache regardless of age
        if (!await NetworkHelpers.InternetAvailable())
        {
            return await LocalCacheSet.ReadCache<IImmutableList<Device>>(path);
        }

        // if all devices requested set validation to 5 minutes
        if (await LocalCacheSet.IsCacheValid(path, TimeSpan.FromMinutes(5)))
        {
            return await LocalCacheSet.ReadCache<IImmutableList<Device>>(path);
        }

        // Otherwise: fetch fresh data
        return null;
    }

    public async ValueTask ClearCache(CancellationToken token)
    {
        await LocalCacheSet.ClearCache(_allDevicesCachePath);
        await LocalCacheSet.ClearCache(_currentDeviceCachePath);
    }
}