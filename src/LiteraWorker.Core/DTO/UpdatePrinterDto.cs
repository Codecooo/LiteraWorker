using LiteraWorker.Core.Models.Enums;

namespace LiteraWorker.Core.DTO;

public class UpdatePrinterDto
{
    public Guid Id { get; set; }
    public PrinterStatus PrinterStatus { get; set; }
    public bool Shared { get; set; }
    public bool Default { get; set; }
}