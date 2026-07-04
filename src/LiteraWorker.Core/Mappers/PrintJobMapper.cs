using LiteraWorker.Core.DTO;
using LiteraWorker.Core.Models;
using Range = SharpIpp.Protocol.Models.Range;

namespace LiteraWorker.Core.Mappers;

public static class PrintJobMapper
{
    public static PrintJob ToPrintJob(this SendPrintJobDto dto)
    {
        return new PrintJob
        {
            Id = Guid.CreateVersion7(),
            DeviceJobId = 0, // This will be set by the print service when the job is sent to the printer
            DeviceSenderId = dto.DeviceSenderId,
            TargetDeviceId = dto.TargetDeviceId,
            UserSenderId = dto.UserSenderId,
            UserReceiverId = dto.UserSenderId,
            Printer = dto.Printer,
            Filename = dto.Filename,
            FilePath = dto.FilePath,
            Route = dto.Route,
            Config = dto.Config.ToPrintConfig(),
            JobStatus = Models.Enums.JobStatus.Uploading,
            Created = DateTime.UtcNow
        };
    }

    public static ServerPrintJobDto ToServerPrintJobDto(this PrintJob printJob)
    {
        return new ServerPrintJobDto
        {
            DeviceJobId = printJob.DeviceJobId, // This will be set by the print service when the job is sent to the printer
            DeviceSenderId = printJob.DeviceSenderId,
            TargetDeviceId = printJob.TargetDeviceId,
            UserSenderId = printJob.UserSenderId,
            UserReceiverId = printJob.UserSenderId,
            Printer = printJob.Printer,
            Filename = printJob.Filename,
            FilePath = printJob.FilePath,
            Route = printJob.Route,
            Config = printJob.Config.ToPrintConfigDto(),
            JobStatus = printJob.JobStatus,
        };
    }

    public static Range[] ToPageRange(this string input)
    {
        var ranges = new List<Range>();

        if (string.IsNullOrWhiteSpace(input))
            return ranges.ToArray();

        var parts = input.Split(',', StringSplitOptions.RemoveEmptyEntries);

        foreach (var part in parts)
        {
            var trimmed = part.Trim();

            if (trimmed.Contains('-'))
            {
                var bounds = trimmed.Split('-', StringSplitOptions.RemoveEmptyEntries);
                if (bounds.Length == 2 &&
                    int.TryParse(bounds[0], out var lower) &&
                    int.TryParse(bounds[1], out var upper) &&
                    lower > 0 && upper >= lower)
                {
                    ranges.Add(new Range(lower, upper));
                }
            }
            else if (int.TryParse(trimmed, out var page) && page > 0)
            {
                ranges.Add(new Range(page, page));
            }
        }

        return ranges.ToArray();
    }

    public static PrintConfig ToPrintConfig(this SendPrintConfigDto printConfigDto)
    {
        return new PrintConfig
        {
            Media = printConfigDto.Media,
            Sides = printConfigDto.Sides,
            Copies = printConfigDto.Copies,
            Color = printConfigDto.Color,
            OrderReversed = printConfigDto.OrderReversed,
            PrintQuality = printConfigDto.PrintQuality,
            PrintOrientation = printConfigDto.PrintOrientation,
            Pages = printConfigDto.Pages != string.Empty ? ToPageRange(printConfigDto.Pages) : []
        };
    }

    public static SendPrintConfigDto ToPrintConfigDto(this PrintConfig printConfig)
    {
        return new SendPrintConfigDto
        {
            Media = printConfig.Media,
            Sides = printConfig.Sides,
            Copies = printConfig.Copies,
            Color = printConfig.Color,
            OrderReversed = printConfig.OrderReversed,
            PrintQuality = printConfig.PrintQuality,
            PrintOrientation = printConfig.PrintOrientation,
            Pages = printConfig.Pages.First().ToString()!.Trim()
        };
    }

    public static PrintJob ToPrintJob(this ServerPrintJobDto dto)
    {
        return new PrintJob
        {
            Id = dto.Id,
            DeviceJobId = dto.DeviceJobId,
            DeviceSenderId = dto.DeviceSenderId,
            TargetDeviceId = dto.TargetDeviceId,
            UserSenderId = dto.UserSenderId,
            UserReceiverId = dto.UserSenderId,
            Printer = dto.Printer,
            Filename = dto.Filename,
            FilePath = dto.FilePath,
            Route = dto.Route,
            Config = dto.Config.ToPrintConfig(),
            JobStatus = dto.JobStatus,
            Created = dto.Created
        };
    }
}