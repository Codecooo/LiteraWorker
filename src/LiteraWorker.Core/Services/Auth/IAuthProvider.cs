using LiteraWorker.Core.DTO;
using LiteraWorker.Core.Helpers;
using LiteraWorker.Core.Services.ApiService;
using PolyType;
using StreamJsonRpc;

namespace LiteraWorker.Core.Services.Auth;

/// <summary>
/// Interface for auth provider to handle authentication in the client side
/// </summary>

[JsonRpcContract, GenerateShape(IncludeMethods = MethodShapeFlags.PublicInstance)]
public partial interface IAuthProvider
{
    /// <summary>
    /// Register user to the API
    /// </summary>
    /// <param name="cancellationToken">Token to cancel the request</param>
    /// <returns>Task to register user</returns>
    ValueTask<Result<EmptyRecord>> RegisterUser(UserDto userDto, CancellationToken cancellationToken = default);

    /// <summary>
    /// Login the user to the API
    /// </summary>
    /// <param name="loginRequest">Object for token request comprising access token and their user ID</param>
    /// <param name="cancellationToken">Token to cancel the request</param>
    /// <returns>Task to login user</returns>
    ValueTask<Result<EmptyRecord>> Login(LoginRequestDto loginRequest, CancellationToken cancellationToken = default);

    /// <summary>
    /// Logout the user to the API
    /// </summary>
    /// <param name="userId">Current user ID</param>
    /// <param name="cancellationToken">Token to cancel the request</param>
    /// <returns>Task to login user</returns>
    ValueTask<Result<EmptyRecord>> Logout(Guid userId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Method to verify if the current user is authenticated in the API 
    /// </summary>
    /// <param name="cancellationToken">Token to cancel the request</param>
    /// <returns>Boolean to indicate if they are authenticated</returns>
    ValueTask<bool> IsAuthenticated(CancellationToken cancellationToken = default);
}