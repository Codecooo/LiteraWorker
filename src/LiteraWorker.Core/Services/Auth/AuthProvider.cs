using LiteraWorker.Core.DTO;
using LiteraWorker.Core.Helpers;
using LiteraWorker.Core.Models;
using LiteraWorker.Core.Services.ApiService;
using LiteraWorker.Core.Services.Caching;
using Microsoft.Extensions.Logging;

namespace LiteraWorker.Core.Services.Auth;

public class AuthProvider(ITokenCache tokenCache, ITokensProvider tokensProvider, ILogger<AuthProvider> logger, AuthService authService, IUserCache userCache, IDeviceCache deviceCache, IPrinterCache printerCache) : IAuthProvider
{
    public async ValueTask<bool> IsAuthenticated(CancellationToken cancellationToken = default)
    {
        try
        {
            var refreshtokens = await tokenCache.GetRefreshTokenAsync(cancellationToken);
            var tokens = await tokensProvider.RefreshTokens(refreshtokens, cancellationToken);
            await tokenCache.SaveAsync(tokens, cancellationToken);
            return true;
        }
        catch (Exception ex) when (ex is IdentityNotFoundException || ex is UnauthorizedAccessException)
        {
            logger.LogError("Refresh token or current identity as a whole is invalid. Please login again");
            return false;
        }
        catch (Exception ex)
        {
            logger.LogError("Unexpected error when trying to determine whether user is authenticated. Please login again. \n Message: {msg}", ex.Message);
            return false;
        }
    }

    public async ValueTask<Result<EmptyRecord>> Login(LoginRequestDto loginRequest, CancellationToken cancellationToken = default)
    {
        var result = await authService.Login(loginRequest, cancellationToken);

        if (!result.Successful)
        {
            return Result<EmptyRecord>.Failure(result.Message, result.StatusCode, result.Problem);
        }

        await tokenCache.SaveAsync(new AuthTokens(result.Value!.TokenResponse.AccessToken, result.Value!.TokenResponse.RefreshToken), cancellationToken);
        await authService.RegisterUserDevice(result.Value!.UserId, cancellationToken);

        return Result<EmptyRecord>.Success(EmptyRecord.Empty);
    }

    public async ValueTask<Result<EmptyRecord>> Logout(Guid userId, CancellationToken cancellationToken = default)
    {
        var result = await authService.Logout(userId, cancellationToken);

        await tokenCache.ClearAsync(cancellationToken);
        await userCache.ClearCache(cancellationToken);
        await deviceCache.ClearCache(cancellationToken);
        await printerCache.ClearCache(cancellationToken);
        
        return result;
    }

    public async ValueTask<Result<EmptyRecord>> RegisterUser(UserDto userDto, CancellationToken cancellationToken = default)
    {
        return await authService.RegisterUser(userDto, cancellationToken);
    }
}