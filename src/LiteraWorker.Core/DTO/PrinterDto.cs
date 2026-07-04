using LiteraWorker.Core.Models;
using LiteraWorker.Core.Models.Enums;

namespace LiteraWorker.Core.DTO;

public class PrinterDto
{
    public Guid Id { get; init; }
    public Guid DeviceId { get; init; }
    public string Name { get; init; } = string.Empty;
    public Uri PrinterUri { get; set; }
    public PrinterStatus PrinterStatus { get; set; }
    public ICollection<Media> SupportedMedia { get; set; } = [];
    public bool Shared { get; set; }
    public bool Default { get; set; }
}