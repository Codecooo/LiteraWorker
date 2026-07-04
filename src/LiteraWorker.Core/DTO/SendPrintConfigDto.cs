using LiteraWorker.Core.Models;
using LiteraWorker.Core.Models.Enums;
using PrintQuality = LiteraWorker.Core.Models.Enums.PrintQuality;
using Range = SharpIpp.Protocol.Models.Range;

namespace LiteraWorker.Core.DTO;

public record SendPrintConfigDto
{
    public Media Media { get; init; }
    public PrintOrientation PrintOrientation { get; init; }
    public PrintQuality PrintQuality { get; init; } = PrintQuality.Normal;
    public PrintSides Sides { get;  init;} = PrintSides.OneSided;
    public int Copies { get; init; }
    public string Pages { get; init; } = string.Empty;
    public bool Color { get; init; } = true;
    public bool OrderReversed { get; init; } = true;
}

