namespace LiteraWorker.Core.DTO;

public class GetConnectedUserDto
{
    public Guid Id { get; init; }
    public string Username { get; init; } = string.Empty;
    public string Email { get; init; } = string.Empty;
}