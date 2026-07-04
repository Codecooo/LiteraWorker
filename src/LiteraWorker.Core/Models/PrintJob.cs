using LiteraWorker.Core.Models.Enums;

namespace LiteraWorker.Core.Models;

public partial record PrintJob
{
    public Guid Id { get; init; }
    public int DeviceJobId { get; init; }
    public Guid DeviceSenderId { get; init; }
    public Guid TargetDeviceId { get; init; }
    public Guid UserSenderId { get; init; }
    public Guid UserReceiverId { get; init; }
    public required Printer Printer { get; init; }
    public PrintRoute Route { get; init; }
    public string Filename { get; init; } = string.Empty;
    public string FilePath { get; init; } = string.Empty;
    public string JobMessage { get; init; } = string.Empty;
    public JobStatus JobStatus { get; init; }
    public DateTime Created { get; init; }
    public required PrintConfig Config { get; init; }
}