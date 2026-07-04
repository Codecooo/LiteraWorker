using LiteraWorker.Core.Helpers;
using LiteraWorker.Core.Models;

namespace LiteraWorker.Core.Services.Auth;

public class TokenCache(IKeyValueStorage keyValueStorage, ITokensProvider tokensProvider) : ITokenCache
{
    private readonly string _key = "litera-auth-tokens";

    public async ValueTask ClearAsync(CancellationToken cancellationToken)
    {
        await keyValueStorage.ClearAsync(_key, cancellationToken);
    }

    public async ValueTask<string> GetAccessTokenAsync(CancellationToken cancellationToken)
    {
        var tokens = await GetTokensAsync(cancellationToken);

        if (JwtDecoder.IsAccessTokenExpired(tokens.AccessToken))
        {
            var newTokens = await tokensProvider.RefreshTokens(tokens.RefreshToken, cancellationToken);
            await keyValueStorage.SetAsync(_key, newTokens, cancellationToken);
            return newTokens.AccessToken;
        }
        
        return tokens.AccessToken;
    }

    public async ValueTask<string> GetRefreshTokenAsync(CancellationToken cancellationToken)
    {
        var tokens = await GetTokensAsync(cancellationToken);
        return tokens.RefreshToken;    
    }

    public async ValueTask SaveAsync(AuthTokens tokens, CancellationToken cancellationToken)
    {
        
        await keyValueStorage.SetAsync(_key, tokens, cancellationToken);
    }

    private async ValueTask<AuthTokens> GetTokensAsync(CancellationToken cancellationToken)
    {
        var tokens = await keyValueStorage.GetAsync<AuthTokens>(_key, cancellationToken) ?? throw new IdentityNotFoundException();
        return tokens;
    }
}