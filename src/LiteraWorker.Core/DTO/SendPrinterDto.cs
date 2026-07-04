using LiteraWorker.Core.Models;
using LiteraWorker.Core.Models.Enums;

namespace LiteraWorker.Core.DTO;

public record SendPrinterDto(
    Guid Id,
    Guid DeviceId,
    string Name,
    string PrinterUri,
    PrinterStatus PrinterStatus,
    ICollection<Media> SupportedMedia,
    bool Shared,
    bool IsDefault);