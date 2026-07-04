using System.Collections.Immutable;
using LiteraWorker.Core.DTO;
using LiteraWorker.Core.Helpers;
using LiteraWorker.Core.Mappers;
using LiteraWorker.Core.Models;

namespace LiteraWorker.Core.Services.ApiService;

public sealed class PrintJobService(HttpService httpService)
{
    public async Task<Result<EmptyRecord>> AddPrintJob(IImmutableList<PrintJob> printJobs)
    {
        return await httpService.PostAsync("api/printjob/add", printJobs.Select(p => p.ToServerPrintJobDto()));
    } 

    public async Task<Result<IImmutableList<ServerPrintJobDto>>> GetPrintJobs(Guid userId)
    {
        return await httpService.GetAsync($"api/printjob/{userId}", JsonContext.Default.IImmutableListServerPrintJobDto);
    }
}