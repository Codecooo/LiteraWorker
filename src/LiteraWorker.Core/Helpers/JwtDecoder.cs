using System.Text.Json;

namespace LiteraWorker.Core.Helpers;

class JwtDecoder
{
    public static bool IsAccessTokenExpired(string token)
    {
        var exp = GetJwtExpiryUtc(token);

        if (exp is null) return false;

        return exp < DateTimeOffset.UtcNow;
    }

    private static DateTimeOffset? GetJwtExpiryUtc(string token)
    {
        if (string.IsNullOrWhiteSpace(token))
            return null;

        // JWT format: header.payload.signature
        var parts = token.Split('.');
        if (parts.Length < 2) // malformed
            return null;

        // The payload is the second part (Base64URL)
        var payload = parts[1];
        // Pad the string for Base64 decoding (Base64URL may omit padding)
        var padded = payload.PadRight(payload.Length + (4 - payload.Length % 4) % 4, '=');
        var jsonBytes = Convert.FromBase64String(padded.Replace('-', '+').Replace('_', '/'));

        using var doc = JsonDocument.Parse(jsonBytes);
        if (!doc.RootElement.TryGetProperty("exp", out var expElem) ||
            expElem.ValueKind != JsonValueKind.Number ||
            !expElem.TryGetInt64(out var secondsSinceEpoch))
            return null;

        return DateTimeOffset.FromUnixTimeSeconds(secondsSinceEpoch);
    }
}