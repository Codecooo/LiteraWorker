using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using LiteraWorker.Core.DTO;
using LiteraWorker.Core.Helpers;
using Microsoft.Extensions.Logging;

namespace LiteraWorker.Core.Services.ApiService;

public sealed class GlobalHttpErrorHandler(ILogger<GlobalHttpErrorHandler> logger) : DelegatingHandler
{
    protected override async Task<HttpResponseMessage> SendAsync(
        HttpRequestMessage request,
        CancellationToken cancellationToken)
    {
        try
        {
            // Forward the request down the pipeline
            var response = await base.SendAsync(request, cancellationToken);

            if (response.StatusCode == HttpStatusCode.TooManyRequests)
            {
                var retryAfter = response.Headers.RetryAfter ?? new RetryConditionHeaderValue(DateTimeOffset.Now.AddSeconds(60));

                // Resolve retry-after to an integer number of seconds 
                var retryAfterSeconds = (int)Math.Ceiling(retryAfter.Delta?.TotalSeconds
                    ?? (retryAfter.Date.HasValue ? (retryAfter.Date.Value - DateTimeOffset.Now).TotalSeconds : 60));

                throw new HttpRequestTooManyException("429: Too many request try again in 1 minute", HttpStatusCode.TooManyRequests, retryAfterSeconds);
            }

            if (!response.IsSuccessStatusCode)
            {
                // if (response.StatusCode == HttpStatusCode.NoContent ||
                //     response.StatusCode == HttpStatusCode.NotFound ||
                //     response.StatusCode == HttpStatusCode.Unauthorized)
                // {
                //     // No JSON payload expected – return a default problem or success result.
                //     throw new HttpRequestFailedException(
                //         $"Unexpected status code {response.StatusCode}",
                //         response.StatusCode,
                //         new ProblemDetails { Title = response.ReasonPhrase });
                // }

                var problem = await response.Content.ReadFromJsonAsync(JsonContext.Default.ProblemDetails, cancellationToken);

                throw new HttpRequestFailedException(
                    $"Unexpected status code {response.StatusCode}",
                    response.StatusCode,
                    problem ?? new ProblemDetails());
            }

            return response;
        }
        catch (TaskCanceledException ex) when (!cancellationToken.IsCancellationRequested)
        {
            logger.LogError(ex, "Request to {Url} timed out.", request.RequestUri);
            throw new HttpRequestTimeoutException($"Request to {request.RequestUri} timed out.", ex);
        }
        catch (HttpRequestException ex)
        {
            // Network‑level failure (DNS, connection refused, etc.)
            logger.LogError(ex, "Network error while calling {Url}.", request.RequestUri);
            throw;
        }
    }
}

public class HttpRequestFailedException(string message, HttpStatusCode statusCode, ProblemDetails problemDetails) : Exception(message)
{
    public HttpStatusCode StatusCode { get; } = statusCode;
    public ProblemDetails ProblemDetails { get; } = problemDetails;
}

public class HttpRequestTimeoutException(string message, Exception inner) : Exception(message, inner);

public class HttpRequestTooManyException(string message, HttpStatusCode statusCode, int retryAfter) : Exception(message)
{
    public HttpStatusCode StatusCode { get; } = statusCode;
    public int RetryAfterSeconds { get; } = retryAfter;
}