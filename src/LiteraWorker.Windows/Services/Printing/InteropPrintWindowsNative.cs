using System.Collections.Immutable;
using System.Runtime.InteropServices;
using LiteraWorker.Core.Models;
using LiteraWorker.Core.Services.Caching;
using static Vanara.PInvoke.WinSpool;

namespace LiteraWorker.Windows.Services.Printing;

public class InteropPrintWindowsNative(IPrinterCache printerCache)
{
    public async Task<IImmutableList<JOB_INFO_2>> GetPrintJobs()
    {
        return await EnumerateJobsAsync();
    }

    public void CancelPrintNative(PrintJob printJob)
    {
        
    }

    private async Task<IImmutableList<JOB_INFO_2>> EnumerateJobsAsync()
    {
        var printers = await printerCache.GetPrinters(CancellationToken.None);

        foreach (var printer in printers)
        {
            OpenPrinter(printer!.Name, out var phPrinter);
            if (phPrinter.IsInvalid) throw new InvalidOperationException($"Failed to open printer for {printer.Name}: {Marshal.GetLastWin32Error}");

            bool ok = EnumJobs(phPrinter,
                FirstJob: 0, NoJobs: 10,
                Level: 2, pJob:
                IntPtr.Zero, cbBuf: 0, out var pcbNeeded, out var pcReturned);

            IntPtr pBuffer = Marshal.AllocHGlobal((int)pcbNeeded);

            try
            {
                ok = EnumJobs(phPrinter,
                FirstJob: 0, NoJobs: 10,
                Level: 2, pJob:
                pBuffer, cbBuf: 0, out pcbNeeded, out pcReturned);

                var jobs = new List<JOB_INFO_2>((int)pcReturned);
                int structSize = Marshal.SizeOf<JOB_INFO_2>();
                IntPtr cur = pBuffer;

                for (int i = 0; i < pcReturned; i++)
                {
                    // Marshal the native struct into a managed copy.
                    JOB_INFO_2 job = Marshal.PtrToStructure<JOB_INFO_2>(cur);
                    SetJob(phPrinter, job.JobId, JOB_CONTROL.JOB_CONTROL_CANCEL);
                    jobs.Add(job);

                    // Advance the pointer to the next struct.
                    cur = IntPtr.Add(cur, structSize);
                }

                return jobs.ToImmutableList();
            }
            catch (System.Exception)
            {

                throw;
            }
            finally
            {
                ClosePrinter(phPrinter);
                Marshal.FreeHGlobal(pBuffer);
            }
        }

        return ImmutableList<JOB_INFO_2>.Empty;
    }
}