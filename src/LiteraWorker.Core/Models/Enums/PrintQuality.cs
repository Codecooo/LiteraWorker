using System.ComponentModel.DataAnnotations;

namespace LiteraWorker.Core.Models.Enums;

public enum PrintQuality
{
    [Display(Name = "Unsupported")]
    Unsupported = 0,

    [Display(Name = "Normal")]
    Draft = 3,
    
    [Display(Name = "Draft")]
    Normal = 4,

    [Display(Name = "High")]
    High = 5
}