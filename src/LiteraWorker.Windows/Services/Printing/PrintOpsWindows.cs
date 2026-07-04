using System.Collections.Immutable;
using LiteraWorker.Core.Models;
using System.Drawing.Printing;
using LiteraWorker.Core.Models.Enums;
using LiteraWorker.Windows.Mappers;
using System.Drawing;
using PdfiumViewer;
using System.Management;
using LiteraWorker.Core.Services.Printing;
using LiteraWorker.Core.Services.Caching;
using System.Collections.Concurrent;
using Microsoft.Extensions.Logging;

namespace LiteraWorker.Windows.Services.Printing;

public sealed class PrintOpsWindows(IDeviceCache deviceCache, IJobsHandler jobsHandler, ILogger<PrintOpsWindows> logger) : IPrintOps
{
    private ConcurrentDictionary<Guid, PrintPageEventArgs> _printEvents = new();

    public Task CancelPrint(PrintJob job, CancellationToken token)
    {
        _printEvents.TryGetValue(job.Id, out var printPageEvent);

        if (printPageEvent is null) return Task.CompletedTask;

        printPageEvent.Cancel = true;

        return Task.CompletedTask;
    }

    public async ValueTask<IImmutableList<Printer>> GetPrintersInfo(CancellationToken token)
    {
        var printers = new List<Printer>();

        var currentDevice = await deviceCache.GetCurrentDevice(token) ?? new Device();

        foreach (string printer in PrinterSettings.InstalledPrinters)
        {
            var ps = new PrinterSettings { PrinterName = printer };

            var newPrinter = new Printer
            {
                Name = ps.PrinterName,
                DeviceId = currentDevice.Id,
                PrinterUri = default, // Windows by default dont provide URI to the printer
                PrinterStatus = QueryManagementInfo(ps.PrinterName, "PrinterStatus").First().ToPrinterStatus(),
                IsDefault = ps.IsDefaultPrinter,
                SupportedMedia = MediaMapper.MapMediaInJson(QueryManagementInfo(ps.PrinterName, "PaperSizesSupported"))
            };

            printers.Add(newPrinter);
        }

        return printers.ToImmutableList();
    }

    public async Task Print(IImmutableList<PrintJob> jobs, CancellationToken token)
    {
        foreach (var job in jobs)
        {
            try
            {
                logger.LogInformation("Starting print for {filename}, {count} jobs left", job.Filename, jobs.Count);
                using var pd = new PrintDocument
                {
                    PrintController = new StandardPrintController()
                };

                // Printer-level settings 
                pd.PrinterSettings.PrinterName = job.Printer.Name;
                pd.PrinterSettings.Copies = (short)job.Config.Copies;
                pd.PrinterSettings.PrintRange = PrintRange.SomePages;
                pd.PrinterSettings.FromPage = job.Config.Pages.First().Lower;
                pd.PrinterSettings.ToPage = job.Config.Pages.First().Upper;
                pd.PrinterSettings.Collate = job.Config.OrderReversed;
                pd.PrinterSettings.Duplex = PrintWindowsMapper.ToDuplexWin(job.Config.Sides);

                pd.DefaultPageSettings.Landscape = job.Config.PrintOrientation == PrintOrientation.Landscape;
                pd.DefaultPageSettings.Color = job.Config.Color;
                pd.DefaultPageSettings.PaperSize = MediaMapper.MapToPaperSize(job.Config.Media, pd.PrinterSettings);

                pd.DocumentName = job.Filename;
                await jobsHandler.AddJobAsync(job);

                // Rendering
                pd.PrintPage += (s, e) =>
                {
                    _printEvents[job.Id] = e;
                    PrintPage(e, job);
                };

                await Task.Run(pd.Print, token);
            }
            catch (Exception ex) when (ex is not OperationCanceledException)
            {
                logger.LogError("Error occured when trying to print {filename}: {msg}", job.Filename, ex.Message);
                throw;
            }
            finally
            {
                _printEvents.TryRemove(job.Id, out _);
            }
        }
    }

    private void PrintPage(PrintPageEventArgs printPageEvent, PrintJob job)
    {
        if (printPageEvent.Cancel) return;

        // get the page ranges to be requested
        var ranges = job.Config.Pages;
        int currentRangeIdx = 0;
        int pageIndex = ranges.ElementAt(0).Lower;

        using var document = PdfDocument.Load(job.FilePath);
        var dpi = job.Config.PrintQuality == PrintQuality.High ? 300 : 150;
        using Image img = document.Render(pageIndex, dpiX: dpi, dpiY: dpi, flags: PdfRenderFlags.Annotations);

        // Compute the scaling factor so that the image fit inside the printable area
        var ratio = Math.Min(
            (float)printPageEvent.MarginBounds.Width / img.Width,
            (float)printPageEvent.MarginBounds.Height / img.Height);

        int width = (int)(img.Width * ratio);
        int height = (int)(img.Height * ratio);

        // calculate the x, y so that the image centered inside the page
        var x = printPageEvent.MarginBounds.X + (printPageEvent.MarginBounds.Width - width) / 2;
        var y = printPageEvent.MarginBounds.Y + (printPageEvent.MarginBounds.Height - height) / 2;

        printPageEvent.Graphics!.DrawImage(img, x, y, width, height);

        if (pageIndex >= ranges.ElementAt(currentRangeIdx).Upper)
        {
            // End of the current range – move to the next one
            currentRangeIdx++;
            if (currentRangeIdx >= ranges.Count())
            {
                printPageEvent.HasMorePages = false;
                return;
            }
            pageIndex = ranges.ElementAt(currentRangeIdx).Lower;
        }
        else
        {
            // Still inside the current range – advance to the next page
            pageIndex++;
        }
        printPageEvent.HasMorePages = true;
    }

    /// <summary>
    /// Method for querying printer information in WMI (Windows Management Information)
    /// </summary>
    /// <param name="printerName">The printer name to be queried</param>
    /// <param name="param">The parameter to specify which property you want to retrieve</param>
    /// <returns>A list of values result from the query in string</returns>
    private static List<string> QueryManagementInfo(string printerName, string param)
    {
        string escapedName = printerName.Replace("\\", "\\\\").Replace("'", "''");
        string query = $"SELECT {param} FROM Win32_Printer WHERE Name = '{escapedName}'";
        var searcher = new ManagementObjectSearcher(query);

        var results = new List<string>();
        foreach (ManagementObject job in searcher.Get().Cast<ManagementObject>())
        {
            results.Add(job.Properties[param]?.Value?.ToString() ?? string.Empty);
        }

        return results;
    }
}