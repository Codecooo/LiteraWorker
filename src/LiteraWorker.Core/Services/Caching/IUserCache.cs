using System.Collections.Immutable;
using LiteraWorker.Core.Helpers;
using LiteraWorker.Core.Models;
using PolyType;
using StreamJsonRpc;

namespace LiteraWorker.Core.Services.Caching;

[JsonRpcContract, GenerateShape(IncludeMethods = MethodShapeFlags.PublicInstance)]
public partial interface IUserCache
{
    ValueTask<Result<User>> GetCurrentUser(CancellationToken token);
    ValueTask<Result<IImmutableList<User>>> GetAllConnectedUsers(CancellationToken token);
    ValueTask ClearCache(CancellationToken token);
}
