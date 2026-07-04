using System.Collections.Immutable;
using LiteraWorker.Core.DTO;
using LiteraWorker.Core.Models;

namespace LiteraWorker.Core.Mappers;

#nullable disable

public static class PrinterMapper
{
    public static List<CreatePrinterDto> ToCreatePrinterList(this IImmutableList<Printer> printers)
    {
        var createPrinterList = new List<CreatePrinterDto>();
        foreach (var printer in printers)
        {
            var createPrinterDto = new CreatePrinterDto
            {
                Name = printer.Name,
                PrinterUri = printer.PrinterUri,
                PrinterStatus = printer.PrinterStatus,
                DeviceId = printer.DeviceId,
                SupportedMedia = printer.SupportedMedia,
                Shared = printer.Shared,
                Default = printer.IsDefault
            };

            createPrinterList.Add(createPrinterDto);
        }

        return createPrinterList;
    }

    public static IImmutableList<Printer> ToPrinterList(this IEnumerable<PrinterDto> printers)
    {
        var createPrinterList = new List<Printer>();
        foreach (var printer in printers)
        {
            var createPrinterDto = new Printer
            {
                Id = printer.Id,
                DeviceId = printer.DeviceId,
                Name = printer.Name,
                PrinterStatus = printer.PrinterStatus,
                PrinterUri = new Uri(printer.PrinterUri.AbsoluteUri),
                SupportedMedia = printer.SupportedMedia.ToMediaList(),

            };

            createPrinterList.Add(createPrinterDto);
        }

        return createPrinterList.ToImmutableArray();
    }

    public static UpdatePrinterDto ToUpdatePrinterDto(this Printer printer)
    {
        return new UpdatePrinterDto
        {
            Id = printer.Id,
            PrinterStatus = printer.PrinterStatus,
            Shared = printer.Shared,
            Default = printer.IsDefault
        };
    }

    public static SendPrinterDto ToSendPrinterDto(this Printer printer)
    {
        return new SendPrinterDto(
            printer.Id,
            printer.DeviceId,
            printer.Name,
            printer.PrinterUri?.AbsoluteUri ?? string.Empty,
            printer.PrinterStatus,
            printer.SupportedMedia ?? [],
            printer.Shared,
            printer.IsDefault
        );
    }

    public static Printer ToPrinter(this SendPrinterDto printerDto)
    {
        return new Printer
        {
            Id = printerDto.Id,
            DeviceId = printerDto.DeviceId,
            Name = printerDto.Name,
            PrinterUri = new Uri(printerDto.PrinterUri),
            PrinterStatus = printerDto.PrinterStatus,
            SupportedMedia = printerDto.SupportedMedia.ToList(),
            Shared = printerDto.Shared,
            IsDefault = printerDto.IsDefault
        };
    }
}