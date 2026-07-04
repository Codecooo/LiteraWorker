using LiteraWorker.Core.Models;

namespace LiteraWorker.Core.Services.Auth;

public interface ITokenCache
{
	/// <summary>
	/// Gets the acces tokens for the current provider.
	/// </summary>
	/// <param name="cancellationToken">
	/// A <see cref="CancellationToken"/> which can be used to cancel the operation.
	/// </param>
	/// <returns>
	/// A task that represents the asynchronous operation. The task result is the tokens for the current provider.
	/// </returns>
	ValueTask<string> GetAccessTokenAsync(CancellationToken cancellationToken);

	/// <summary>
	/// Gets the refresh tokens for the current provider.
	/// </summary>
	/// <param name="cancellationToken">
	/// A <see cref="CancellationToken"/> which can be used to cancel the operation.
	/// </param>
	/// <returns>
	/// A task that represents the asynchronous operation. The task result is the tokens for the current provider.
	/// </returns>
	ValueTask<string> GetRefreshTokenAsync(CancellationToken cancellationToken);

	/// <summary>
	/// Saves a dictionary of tokens for the specified provider.
	/// </summary>
	/// <param name="tokens">
	/// A dictionary of tokens to save.
	/// </param>
	/// <param name="cancellationToken">
	/// A <see cref="CancellationToken"/> which can be used to cancel the operation.
	/// </param>
	/// <returns>
	/// A task that represents the asynchronous operation.
	/// </returns>
	ValueTask SaveAsync(AuthTokens tokens, CancellationToken cancellationToken);

	/// <summary>
	/// Clears the tokens for the current provider.
	/// </summary>
	/// <param name="cancellationToken">
	/// A <see cref="CancellationToken"/> which can be used to cancel the operation.
	/// </param>
	/// <returns>
	/// A task that represents the asynchronous operation.
	/// </returns>
	ValueTask ClearAsync(CancellationToken cancellationToken);
}