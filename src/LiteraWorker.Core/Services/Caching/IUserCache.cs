using System.Collections.Immutable;
using LiteraWorker.Core.Helpers;
using LiteraWorker.Core.Models;

namespace LiteraWorker.Core.Services.Caching;

public interface IUserCache
{
    ValueTask<Result<User>> GetCurrentUser(CancellationToken token);
    ValueTask<Result<IImmutableList<User>>> GetAllConnectedUsers(CancellationToken token);
    ValueTask ClearCache(CancellationToken token);
}
