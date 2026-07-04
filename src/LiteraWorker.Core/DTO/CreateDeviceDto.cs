using LiteraWorker.Core.Models.Enums;

namespace LiteraWorker.Core.DTO;

public class CreateDeviceDto
{
    public Guid UserId { get; init; }
    public string Name { get; init; } = string.Empty;
    public string OS { get; init; } = string.Empty;
    public string IpAddress { get; init; } = string.Empty;
    public DevicesStatus DeviceStatus { get; init; }
    public bool AllowedToInternet { get; init; }
    public bool AllowedPeerToPeer { get; init; }
    public bool CanSend { get; init; }
    public bool CanReceive { get; init; }
    public Guid Fingerprint { get; init; }
}