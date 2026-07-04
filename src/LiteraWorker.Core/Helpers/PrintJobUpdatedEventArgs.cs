using LiteraWorker.Core.Models;

namespace LiteraWorker.Core.Helpers;

public class PrintJobUpdatedEventArgs(List<PrintJob> printJobs) : EventArgs
{
    public List<PrintJob> PrintJobs { get; set; } = printJobs;
}