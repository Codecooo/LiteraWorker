using LiteraWorker.Core.Helpers;
using LiteraWorker.Core.Models;

namespace LiteraWorker.Core.Services.Auth;

public class TokenCache(IKeyValueStorage keyValueStorage, ITokensProvider tokensProvider) : ITokenCache
{
    private readonly string _filePath = OperatingSystem.IsWindows() ? Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "LiteraWorker", "auth_tokens.json")
        : "/var/lib/litera/auth_tokens.json";

    public async ValueTask ClearAsync(CancellationToken cancellationToken)
    {
        await keyValueStorage.ClearAsync(_filePath, cancellationToken);
    }

    public async ValueTask<string> GetAccessTokenAsync(CancellationToken cancellationToken)
    {
        var tokens = await GetTokensAsync(cancellationToken);

        if (JwtDecoder.IsAccessTokenExpired(tokens.AccessToken))
        {
            var newTokens = await tokensProvider.RefreshTokens(tokens.RefreshToken, cancellationToken);
            await keyValueStorage.SetAsync(_filePath, newTokens, cancellationToken);
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
        
        await keyValueStorage.SetAsync(_filePath, tokens, cancellationToken);
    }

    private async ValueTask<AuthTokens> GetTokensAsync(CancellationToken cancellationToken)
    {
        var tokens = await keyValueStorage.GetAsync<AuthTokens>(_filePath, cancellationToken) ?? throw new IdentityNotFoundException();
        return tokens;
    }
}