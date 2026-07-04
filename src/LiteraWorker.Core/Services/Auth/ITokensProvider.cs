using LiteraWorker.Core.Models;

namespace LiteraWorker.Core.Services.Auth;

public interface ITokensProvider
{
    Task<AuthTokens> RefreshTokens(string refreshToken, CancellationToken cancellationToken = default);    
}