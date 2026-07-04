namespace LiteraWorker.Core.DTO;

public record LoginResponseDto(Guid UserId, TokenResponseDto TokenResponse);