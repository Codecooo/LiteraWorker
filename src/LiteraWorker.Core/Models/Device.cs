using LiteraWorker.Core.Models.Enums;

namespace LiteraWorker.Core.Models;

public partial record Device
{
    public Guid Id { get; init; }
    public string Name { get; init; } = string.Empty;
    public string OS { get; init; } = string.Empty;
    public string IpAddress { get; init; } = string.Empty;
    public DevicesStatus DeviceStatus { get; init; }
    public bool AllowedToInternet { get; init; } = true;
    public bool AllowedPeerToPeer { get; init; } = true;
    public bool CanSend { get; init; } = true;
    public bool CanReceive { get; init; } 
    public Guid UserId { get; init; }
}