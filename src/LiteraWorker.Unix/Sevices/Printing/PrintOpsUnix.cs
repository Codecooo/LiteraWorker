using System.Collections.Immutable;
using LiteraWorker.Unix.Mappers;
using LiteraWorker.Core.Models;
using LiteraWorker.Core.Models.Enums;
using LiteraWorker.Core.Services.Caching;
using SharpIpp;
using SharpIpp.Models;
using LiteraWorker.Core.Mappers;
using LiteraWorker.Core.Services.Printing;
using Microsoft.Extensions.Logging;
using System.Net;
using SharpIpp.Protocol.Models;
using LiteraWorker.Core.Services.Auth;

namespace LiteraWorker.Unix.Sevices.Printing;

public sealed class PrintOpsUnix(IDeviceCache deviceCache, IJobsHandler jobsHandler, LocalAuthHandler localAuthHandler, ILogger<PrintOpsUnix> logger) : IPrintOps
{
    private static readonly Uri _cupsUri = new(Environment.GetEnvironmentVariable("CUPS_SERVER") ?? "http://localhost:631");

    private async Task<SharpIppClient> InitializeClient()
    {
        if (!CUPSAvailable("lpstat") || !CUPSAvailable("lp"))
        {
            throw new FileNotFoundException("CUPS is not installed or not configured to system root");
        }

        var httpHandler = new HttpClientHandler
        {
            Credentials = await localAuthHandler.GetUser() 
        };
        
        var httpClient = new HttpClient(httpHandler);
        return new SharpIppClient(_cupsUri, httpClient);
    }

    /// <summary>
    ///     Get printers information  from name, status, and default printer from CUPS 
    /// </summary>
    /// <returns></returns>
    public async ValueTask<IImmutableList<Printer>> GetPrintersInfo(CancellationToken token)
    {
        var printersInfo = new List<Printer>();
        var currentDevice = await deviceCache.GetCurrentDevice(token);

        List<string> printerNames = await GetPrinterAttributes(reqAttributes: ["printer-name"], uri: _cupsUri, token);

        for (int i = 0; i < printerNames.Count; i++)
        {
            List<string> printerUris = await GetPrinterAttributes(reqAttributes: ["printer-uri-supported"], uri: _cupsUri, token);

            var currentPrinterUri = new Uri(printerUris[i]);
            List<string> medias = await GetPrinterAttributes(reqAttributes: ["media-supported"], uri: currentPrinterUri, token);
            List<string> printerStatus = await GetPrinterAttributes(reqAttributes: ["printer-state"], uri: currentPrinterUri, token);

            var printer = new Printer
            {
                Name = printerNames[i],
                PrinterUri = currentPrinterUri,
                PrinterStatus = (PrinterStatus)int.Parse(printerStatus[i]),
                Shared = true,
                DeviceId = currentDevice.Value!.Id,
                SupportedMedia = MediaMapper.MapMediaInJson(medias) ?? Enumerable.Empty<Media>().ToList()
            };

            printersInfo.Add(printer);
        }

        return printersInfo.ToImmutableList();
    }

    /// <summary>
    ///     Start printing in CUPS by sending print jobs to CUPS server
    /// </summary>
    /// <param name="printJobs">print job configuration</param>
    /// <param name="token">token for cancelling operation</param>
    /// <exception cref="FileNotFoundException">Thrown when CUPS is not installed on the system</exception>
    public async Task Print(IImmutableList<PrintJob> printJobs, CancellationToken token)
    {
        using var client = await InitializeClient();

        foreach (var printJob in printJobs)
        {
            if (token.IsCancellationRequested) break;

            try
            {
                logger.LogInformation("Starting print for {filename}, {count} jobs left", printJob.Filename, printJobs.Count);
                using var stream = File.OpenRead(printJob.FilePath);

                if (!File.Exists(printJob.FilePath))
                    throw new FileNotFoundException(printJob.FilePath);

                var attributes = new IppAttribute[]
                {
                    new(Tag.NameWithoutLanguage, "ColorModel",printJob.Config.Color ? "RGB" : "Gray"),                    
                    new(Tag.NameWithoutLanguage, "job-originating-user-name", Environment.UserName),
                    new(Tag.NameWithoutLanguage, "cupsPrintQuality", printJob.Config.PrintQuality.ToString())
                };

                var request = new PrintJobRequest
                {
                    PrinterUri = printJob.Printer.PrinterUri ?? throw new InvalidOperationException("Printer URI cannot be null"),
                    Document = stream,
                    NewJobAttributes = new()
                    {
                        JobName = printJob.Filename,
                        Copies = printJob.Config.Copies,
                        Media = printJob.Config.Media.RawName,
                        PageRanges = printJob.Config.Pages,
                        OrientationRequested = printJob.Config.PrintOrientation.ToSharpIpp(),
                        AdditionalJobAttributes = attributes
                    },
                };

                var response = await client.PrintJobAsync(request, token);

                await jobsHandler.AddJobAsync(printJob, response);
            }
            catch (Exception ex) when (ex is not OperationCanceledException)
            {
                logger.LogError("Error occured when trying to print {filename}: {msg}", printJob.Filename, ex.Message);
                throw;
            }
        }
    }

    private async Task<List<string>> GetPrinterAttributes(string[] reqAttributes, Uri uri, CancellationToken token)
    {
        using var client = await InitializeClient();

        var request = new CUPSGetPrintersRequest
        {
            PrinterUri = uri,
            RequestedAttributes = reqAttributes
        };

        var response = await client.GetCUPSPrintersAsync(request, token);
        var values = new List<string>();

        foreach (var value in response.Sections)
        {
            foreach (var item in value.Attributes)
            {
                if (item.Value.ToString() == "utf-8" || item.Value.ToString() == "en")
                    continue;

                values.Add(item.Value.ToString());
            }
        }

        return values;
    }

    public async Task CancelPrint(PrintJob job, CancellationToken token)
    {
        using var client = await InitializeClient();

        var cancelReq = new CancelJobRequest
        {
            JobId = job.DeviceJobId,
            PrinterUri = job.Printer.PrinterUri!
        };
        await client.CancelJobAsync(cancelReq, token);
    }

    private static bool CUPSAvailable(string executable)
    {
        var searchPaths = new[] { "/usr/bin", "/usr/sbin", "/bin", "/sbin" };

        return searchPaths.Any(dir => File.Exists(Path.Combine(dir, executable)));
    }
}