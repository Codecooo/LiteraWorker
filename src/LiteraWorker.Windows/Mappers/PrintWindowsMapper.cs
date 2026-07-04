using System.Collections.Immutable;
using System.Drawing.Printing;
using LiteraWorker.Core.Models;
using LiteraWorker.Core.Models.Enums;
using Vanara.PInvoke;

namespace LiteraWorker.Windows.Mappers;

public static class PrintWindowsMapper
{
    public static Duplex ToDuplexWin(this PrintSides printSides)
    {
        return printSides switch
        {
            PrintSides.DoubleSided => Duplex.Horizontal,
            PrintSides.OneSided => Duplex.Simplex,
            _ => Duplex.Default
        };
    }

    public static PrinterStatus ToPrinterStatus(this string status)
    {
        return status switch
        {
            "1" => PrinterStatus.Other,
            "3" => PrinterStatus.Idle,
            "4" => PrinterStatus.Processing,
            "6" => PrinterStatus.Stopped,
            "7" => PrinterStatus.Offline,
            _ => PrinterStatus.Other
        };
    }

    public static PrintJob ToPrintJob(this WinSpool.JOB_INFO_2 jobInfo, List<PrintJob> printJobs)
    {
        return printJobs.FirstOrDefault(pj => pj.DeviceJobId == jobInfo.JobId)!;
    }

    public static IImmutableList<PrintJob> ToImmutablePrintJobs(this IImmutableList<WinSpool.JOB_INFO_2> jobsInfoNative, List<PrintJob> knownPrintJobs)
    {
        var jobs = new List<PrintJob>();

        foreach (var jobInfoNative in jobsInfoNative)
        {
            jobs.Add(jobInfoNative.ToPrintJob(knownPrintJobs));
        }

        return jobs.ToImmutableList();
    }
}