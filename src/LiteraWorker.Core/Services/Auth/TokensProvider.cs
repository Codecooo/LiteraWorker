using System.Net.Http.Json;
using LiteraWorker.Core.DTO;
using LiteraWorker.Core.Helpers;
using LiteraWorker.Core.Models;
using LiteraWorker.Core.Services.Caching;

namespace LiteraWorker.Core.Services.Auth;

public class TokensProvider(IHttpClientFactory httpClientFactory, IPersistentIdentity identity) : ITokensProvider
{
    public async Task<AuthTokens> RefreshTokens(string refreshToken, CancellationToken cancellationToken = default)
    {
        using var client = httpClientFactory.CreateClient("AuthClient");
        var user = await identity.LoadIdentity(cancellationToken) ?? throw new IdentityNotFoundException();

        var result = await client.PostAsJsonAsync("api/auth/refresh-tokens", refreshToken, cancellationToken);

        if (!result.IsSuccessStatusCode)
        {
            throw new UnauthorizedAccessException();
        }

        var tokens = await result.Content.ReadFromJsonAsync(JsonContext.Default.TokenResponseDto, cancellationToken);
        
        return new AuthTokens(tokens!.AccessToken, tokens.RefreshToken);
    }
}