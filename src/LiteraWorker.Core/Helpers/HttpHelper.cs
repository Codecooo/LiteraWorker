using System.Net.Http.Json;
using LiteraWorker.Core.DTO;

namespace LiteraWorker.Core.Helpers;

public class HttpHelper
{
    public static async Task<(bool success, HttpResponseMessage response, ProblemDetails? problemDetails)> ValidateResponseAsync(HttpResponseMessage response)
    {
        if (!response.IsSuccessStatusCode)
        {
            var problemDetails = await response.Content.ReadFromJsonAsync(JsonContext.Default.ProblemDetails);
            return (false, response, problemDetails);
        }
        return (true, response, null);
    }
}