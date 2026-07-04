using System.Collections.Immutable;
using System.Net;
using LiteraWorker.Core.Helpers;
using LiteraWorker.Core.Models;
using LiteraWorker.Core.Services.ApiService;
using Microsoft.Extensions.Logging;

namespace LiteraWorker.Core.Services.Caching;

public sealed class UserCache(
    ILogger<UserCache> logger,
    UserService userService,
    IPersistentIdentity persistentIdentity) : IUserCache
{
    private readonly string _currentUserCachePath = LocalCacheSet.InitializeCachePath("current-user.json");
    private readonly string _allUserCachePath = LocalCacheSet.InitializeCachePath("users.json");

    public async ValueTask<Result<User>> GetCurrentUser(CancellationToken token)
    {
        var user = await GetCachedUser(token, reqCurrentUser: true);
        if (user != null)
        {
            return Result<User>.Success(user.First());
        }

        var identity = await persistentIdentity.LoadIdentity(token);
        if (!await NetworkHelpers.InternetAvailable())
        {
            logger.LogWarning("App is offline and cannot connect to the API.");
            throw new WebException("No internet connection", WebExceptionStatus.ConnectFailure);
        }
        if (identity is null)
        {
            throw new IdentityNotFoundException();
        }

        var userResult = await userService.GetCurrentUser(identity.UserId, token);

        var userList = ImmutableArray.Create(userResult.Value);
        await LocalCacheSet.WriteCache(userList, _currentUserCachePath);
        return userResult;
    }

    public async ValueTask<Result<IImmutableList<User>>> GetAllConnectedUsers(CancellationToken token)
    {
        var user = await GetCachedUser(token, reqCurrentUser: false);
        if (user != null)
        {
            return Result<IImmutableList<User>>.Success(user);
        }

        if (!await NetworkHelpers.InternetAvailable())
        {
            logger.LogWarning("App is offline and cannot connect to the API.");
            throw new WebException("No internet connection", WebExceptionStatus.ConnectFailure);
        }

        var currentUser = await GetCurrentUser(token);
        
        var users = await userService.GetConnectedUsers(currentUser.Value!.Id, token);

        await LocalCacheSet.WriteCache(users.Value!, _allUserCachePath);
        return users;
    }

    private async Task<IImmutableList<User>?> GetCachedUser(
        CancellationToken token, bool reqCurrentUser)
    {
        var path = reqCurrentUser ? _currentUserCachePath : _allUserCachePath;

        // If explicitly requesting current device,
        // allow cache up to 1 day old
        if (reqCurrentUser && await LocalCacheSet.IsCacheValid(path, TimeSpan.FromDays(1)))
        {
            return await LocalCacheSet.ReadCache<ImmutableArray<User>>(path);
        }

        // If offline, reuse cache regardless of age
        if (!await NetworkHelpers.InternetAvailable())
        {
            return await LocalCacheSet.ReadCache<ImmutableArray<User>>(path);
        }

        if (await LocalCacheSet.IsCacheValid(path, TimeSpan.FromMinutes(5)))
        {
            return await LocalCacheSet.ReadCache<ImmutableArray<User>>(path);
        }

        // Otherwise: fetch fresh data
        return null;
    }

    public async ValueTask ClearCache(CancellationToken token)
    {
        await LocalCacheSet.ClearCache(_allUserCachePath);
        await LocalCacheSet.ClearCache(_currentUserCachePath);
    }
}