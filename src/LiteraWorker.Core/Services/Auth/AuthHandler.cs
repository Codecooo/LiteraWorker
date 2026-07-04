using System.Net.Http.Headers;
using Microsoft.Extensions.Logging;

namespace LiteraWorker.Core.Services.Auth;

public class AuthHandler(ITokenCache tokenCache, ILogger<AuthHandler> logger) : DelegatingHandler
{
    protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
    {
        try
        {
            var accessToken = await tokenCache.GetAccessTokenAsync(cancellationToken);
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);
            return await base.SendAsync(request, cancellationToken);
        }
        catch (UnauthorizedAccessException)
        {
            logger.LogError("Error trying to get access token because refresh token is expired. Please login again in the main app");
        }
        catch (Exception ex)
        {
            logger.LogError("Error when trying to get access token: {msg}", ex.Message);
        }

        return await base.SendAsync(request, cancellationToken);
    }
}