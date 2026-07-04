using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Serialization.Metadata;
using LiteraWorker.Core.DTO;
using LiteraWorker.Core.Helpers;
using Microsoft.Extensions.Logging;

namespace LiteraWorker.Core.Services.ApiService;

public class HttpService(
    IHttpClientFactory httpClientFactory,
    ILogger<HttpService> logger)
{
    #region GET

    public async Task<Result<TResponse>> GetAsync<TResponse>(
        string route,
        JsonTypeInfo<TResponse> jsonTypeInfo,
        CancellationToken token = default)
    {
        using var client = httpClientFactory.CreateClient("LiteraClient");

        try
        {
            using var response = await client.GetAsync(route, token);

            var handled = await HandleResponse(response);
            if (!handled.Successful)
                return Result<TResponse>.Failure(
                    handled.Message,
                    handled.StatusCode,
                    handled.Problem);

            var json = await response.Content.ReadAsStringAsync();

            var result = JsonSerializer.Deserialize(json, jsonTypeInfo);
            if (result is null)
            {
                return Result<TResponse>.Failure("Failed to read response", 500, new ProblemDetails());
            }

            return Result<TResponse>.Success(result);
        }
        catch (HttpRequestFailedException ex)
        {
            logger.LogError(ex, "GET request failed for {Route}", route);
            return Result<TResponse>.Failure(ex.Message, (int)ex.StatusCode, ex.ProblemDetails);
        }
    }

    #endregion

    #region POST - With Response Body

    public async Task<Result<TResponse>> PostAsync<TRequest, TResponse>(
        string route,
        TRequest body,
        JsonTypeInfo<TResponse> jsonResponseTypeInfo,
        CancellationToken token = default)
    {
        return await SendWithBodyAsync<TRequest, TResponse>(
            route,
            HttpMethod.Post,
            body,
            jsonResponseTypeInfo,
            token);
    }

    #endregion

    #region POST - Fire and Forget (No Response Body Expected)

    /// <summary>
    /// POST request that doesn't expect a JSON response body.
    /// Returns success if status code indicates success (2xx).
    /// </summary>
    public async Task<Result<EmptyRecord>> PostAsync<TRequest>(
        string route,
        TRequest body,
        CancellationToken token = default)
    {
        return await SendWithoutResponseAsync<TRequest>(
            route,
            HttpMethod.Post,
            body,
            token);
    }

    #endregion

    #region PUT - With Response Body

    public async Task<Result<TResponse>> PutAsync<TRequest, TResponse>(
        string route,
        TRequest body,
        JsonTypeInfo<TResponse> jsonResponseTypeInfo,
        CancellationToken token = default)
    {
        return await SendWithBodyAsync<TRequest, TResponse>(
            route,
            HttpMethod.Put,
            body,
            jsonResponseTypeInfo,
            token);
    }

    #endregion

    #region PUT - Fire and Forget

    public async Task<Result<EmptyRecord>> PutAsync<TRequest>(
        string route,
        TRequest body,
        CancellationToken token = default)
    {
        return await SendWithoutResponseAsync<TRequest>(
            route,
            HttpMethod.Put,
            body,
            token);
    }

    #endregion

    #region DELETE

    public async Task<Result<EmptyRecord>> DeleteAsync(
        string route,
        CancellationToken token = default)
    {
        using var client = httpClientFactory.CreateClient("LiteraClient");

        try
        {
            using var response = await client.DeleteAsync(route, token);

            var handled = await HandleResponse(response);
            if (!handled.Successful)
                return Result<EmptyRecord>.Failure(
                    handled.Message,
                    handled.StatusCode,
                    handled.Problem);

            return Result<EmptyRecord>.Success(EmptyRecord.Empty);
        }
        catch (HttpRequestFailedException ex)
        {
            logger.LogError(ex, "DELETE request failed for {Route}", route);
            return Result<EmptyRecord>.Failure(ex.Message, (int)ex.StatusCode, ex.ProblemDetails);
        }
    }

    #endregion

    #region Shared Body Logic (Source Generator Safe)

    private async Task<Result<TResponse>> SendWithBodyAsync<TRequest, TResponse>(
        string route,
        HttpMethod method,
        TRequest body,
        JsonTypeInfo<TResponse> jsonResponseTypeInfo,
        CancellationToken token)
    {
        using var client = httpClientFactory.CreateClient("LiteraClient");

        try
        {
            using var request = new HttpRequestMessage(method, route)
            {
                Content = JsonContent.Create(body, options: JsonOptionsDefault.Options)
            };

            using var response = await client.SendAsync(request, token);

            var handled = await HandleResponse(response);
            if (!handled.Successful)
                return Result<TResponse>.Failure(
                    handled.Message,
                    handled.StatusCode,
                    handled.Problem);

            var json = await response.Content.ReadAsStringAsync();
            
            // Handle empty response body gracefully
            if (string.IsNullOrWhiteSpace(json))
            {
                // Return default value for reference types (null for TResponse)
                // This allows callers to handle the empty case
                return Result<TResponse>.Success(default!);
            }

            var result = JsonSerializer.Deserialize(json, jsonResponseTypeInfo);

            return Result<TResponse>.Success(result!);
        }
        catch (HttpRequestFailedException ex)
        {
            logger.LogError(ex,
                "{Method} request failed for {Route}",
                method,
                route);

            return Result<TResponse>.Failure(ex.Message, (int)ex.StatusCode, ex.ProblemDetails);
        }
    }

    #endregion

    #region Send Without Response Body

    /// <summary>
    /// Sends a request without expecting a JSON response body.
    /// Validates that the response status code indicates success.
    /// </summary>
    private async Task<Result<EmptyRecord>> SendWithoutResponseAsync<TRequest>(
        string route,
        HttpMethod method,
        TRequest body,
        CancellationToken token)
    {
        using var client = httpClientFactory.CreateClient("LiteraClient");

        try
        {
            using var request = new HttpRequestMessage(method, route)
            {
                Content = JsonContent.Create(body, options: JsonOptionsDefault.Options)
            };

            using var response = await client.SendAsync(request, token);

            var handled = await HandleResponse(response);
            if (!handled.Successful)
                return Result<EmptyRecord>.Failure(
                    handled.Message,
                    handled.StatusCode,
                    handled.Problem);

            // Success - no body to deserialize
            return Result<EmptyRecord>.Success(EmptyRecord.Empty);
        }
        catch (HttpRequestFailedException ex)
        {
            logger.LogError(ex,
                "{Method} request failed for {Route}",
                method,
                route);

            return Result<EmptyRecord>.Failure(ex.Message, (int)ex.StatusCode, ex.ProblemDetails);
        }
    }

    #endregion

    #region Response Handling

    private async Task<Result<EmptyRecord>> HandleResponse(HttpResponseMessage response)
    {
        if (response.IsSuccessStatusCode)
            return Result<EmptyRecord>.Success(EmptyRecord.Empty);

        ProblemDetails? problem = null;

        try
        {
            var json = await response.Content.ReadAsStringAsync();

            problem = JsonSerializer.Deserialize(json, JsonContext.Default.ProblemDetails);
        }
        catch
        {
            // Ignore deserialization failure
        }

        var message = problem?.Detail
                      ?? problem?.Title
                      ?? response.ReasonPhrase
                      ?? "Request failed";

        logger.LogWarning(
            "HTTP {StatusCode} - {Message}",
            (int)response.StatusCode,
            message);

        return Result<EmptyRecord>.Failure(
            message,
            (int)response.StatusCode,
            problem);
    }

    #endregion
}

public record EmptyRecord
{
    public static EmptyRecord Empty { get; } = new();
}