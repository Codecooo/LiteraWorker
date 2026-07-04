using LiteraWorker.Core.Models;
using LiteraWorker.Core.Models.Enums;

namespace LiteraWorker.Core.DTO;

public class CreatePrinterDto
{
    public Guid DeviceId { get; set; }
    public string Name { get; init; } = string.Empty;
    public Uri PrinterUri { get; set; }
    public PrinterStatus PrinterStatus { get; set; }
    public ICollection<Media> SupportedMedia { get; set; } = [];
    public bool Shared { get; set; }
    public bool Default { get; set; }
}