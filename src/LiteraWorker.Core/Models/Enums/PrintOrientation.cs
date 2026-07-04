using System.ComponentModel.DataAnnotations;

namespace LiteraWorker.Core.Models.Enums;

public enum PrintOrientation
{
    [Display(Name = nameof(Unsupported))]
    Unsupported = 0,
    [Display(Name = nameof(Portrait))]
    Portrait = 3,
    [Display(Name = nameof(Landscape))]
    Landscape = 4,
    [Display(Name = "Reverse Landscape")]
    ReverseLandscape = 5,
    [Display(Name = "Reverse Portrait")]
    ReversePortrait = 6
}
