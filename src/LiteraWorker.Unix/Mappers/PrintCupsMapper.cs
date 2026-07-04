using LiteraWorker.Core.Models.Enums;
using SharpIpp.Protocol.Models;
using PrintQuality = LiteraWorker.Core.Models.Enums.PrintQuality;

namespace LiteraWorker.Unix.Mappers;

public static class PrintCupsMapper
{
    public static SharpIpp.Protocol.Models.PrintQuality ToSharpIpp(this PrintQuality printQuality)
    {
        return printQuality switch
        {
            PrintQuality.Draft => SharpIpp.Protocol.Models.PrintQuality.Draft,
            PrintQuality.Normal => SharpIpp.Protocol.Models.PrintQuality.Normal,
            PrintQuality.High => SharpIpp.Protocol.Models.PrintQuality.High,
            _ => SharpIpp.Protocol.Models.PrintQuality.Unsupported,
        };
    }

    public static Orientation ToSharpIpp(this PrintOrientation printOrientation)
    {
        return printOrientation switch
        {
            PrintOrientation.Landscape => SharpIpp.Protocol.Models.Orientation.Landscape,
            PrintOrientation.Portrait => SharpIpp.Protocol.Models.Orientation.Portrait,
            PrintOrientation.ReverseLandscape => SharpIpp.Protocol.Models.Orientation.ReverseLandscape,
            PrintOrientation.ReversePortrait => SharpIpp.Protocol.Models.Orientation.ReversePortrait,
            _ => SharpIpp.Protocol.Models.Orientation.Unsupported,
        };
    }

    public static JobStatus ToJobStatus(this JobState jobState)
    {
        return jobState switch
        {
            JobState.Pending => JobStatus.Pending,
            JobState.Processing => JobStatus.Printing,
            JobState.Canceled => JobStatus.Canceled,
            JobState.Completed => JobStatus.Completed,
            _ => JobStatus.Failed
        };
    }
}