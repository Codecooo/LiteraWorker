using System.Collections.Immutable;
using LiteraWorker.Core.DTO;
using LiteraWorker.Core.Helpers;
using LiteraWorker.Core.Mappers;
using LiteraWorker.Core.Models;

namespace LiteraWorker.Core.Services.ApiService;

public sealed class UserService(HttpService httpService)
{
    public async ValueTask<Result<IImmutableList<User>>> GetConnectedUsers(Guid userId, CancellationToken token)
    {
        var result = await httpService.GetAsync($"api/user/connect/get/{userId}", JsonContext.Default.IImmutableListUserDto, token);

        if (!result.Successful)
        {
            return Result<IImmutableList<User>>.Failure(result.Message, result.StatusCode, result.Problem);
        }

        return Result<IImmutableList<User>>.Success(result.Value!.Select(u => u.ToUser()).ToImmutableArray());    
    }

    public async ValueTask<Result<User>> GetCurrentUser(Guid userId, CancellationToken token)
    {
        var query = $"userId={userId}";
        var result = await httpService.GetAsync($"api/user?{query}", JsonContext.Default.UserDto, token);

        if (!result.Successful)
        {
            return Result<User>.Failure(result.Message, result.StatusCode, result.Problem);
        }

        return Result<User>.Success(result.Value!.ToUser());
    }
}