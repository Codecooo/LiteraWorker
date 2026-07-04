namespace LiteraWorker.Core.Models;

public record Identity
{
    public Guid UserId { get; init; }
    public Guid DeviceId { get; init; }
    public DateTime CreatedAt { get; init; }
    public DateTime ModifiedAt { get; init; }
    public Guid Fingerprint { get; init; }
}