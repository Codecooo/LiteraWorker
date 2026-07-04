using System.ComponentModel.DataAnnotations;

namespace LiteraWorker.Core.Models.Enums;

public enum PrinterStatus
{
    [Display(Name = "Idle")]
    Idle = 3,
    [Display(Name = "Processing")]

    Processing = 4,
    [Display(Name = "Stopped")]

    Stopped = 5,
    [Display(Name = "Offline")]
    Offline = 6,
    Other = 7
}