namespace LiteraWorker.Core.DTO;

public record TokenRequestDto(Guid UserId, string RefreshToken);