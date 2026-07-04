using LiteraWorker.Core.Models.Enums;
using PrintQuality = LiteraWorker.Core.Models.Enums.PrintQuality;
using Range = SharpIpp.Protocol.Models.Range;

namespace LiteraWorker.Core.Models;

public record PrintConfig
{
    public Media Media { get; init; }
    public PrintOrientation PrintOrientation { get; init; }
    public PrintQuality PrintQuality { get; init; } = PrintQuality.Normal;
    public PrintSides Sides { get;  init;} = PrintSides.OneSided;
    public int Copies { get; init; }
    public Range[] Pages { get; init; }
    public bool Color { get; init; } = true;
    public bool OrderReversed { get; init; } = true;
}