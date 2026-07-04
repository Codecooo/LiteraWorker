using System.Text.Json.Serialization;

namespace LiteraWorker.Core.DTO;

public record TokenResponseDto(string AccessToken, string RefreshToken);