using System.Collections.Immutable;
using LiteraWorker.Core.Helpers;
using LiteraWorker.Core.Mappers;
using LiteraWorker.Core.Models;
using Microsoft.Extensions.Logging;

namespace LiteraWorker.Core.Services.ApiService;

public sealed class PrinterService(HttpService httpService, ILogger<PrinterService> logger)
{

    public async ValueTask<Result<IImmutableList<Printer>>> GetPrinters(Guid userId, CancellationToken token)
    {
        var result = await httpService.GetAsync($"api/printer/{userId}", JsonContext.Default.IImmutableListPrinterDto, token);

        if (!result.Successful)
        {
            return Result<IImmutableList<Printer>>.Failure(result.Message, result.StatusCode, result.Problem);
        }

        return Result<IImmutableList<Printer>>.Success(result.Value.ToPrinterList());
    }

    public async ValueTask<Result<IImmutableList<Printer>>> RegisterPrinters(IImmutableList<Printer> printers)
    {
        var createPrintets = printers.ToCreatePrinterList();
        var result = await httpService.PostAsync("api/printer/new", createPrintets, JsonContext.Default.IImmutableListPrinterDto);

        if (!result.Successful)
        {
            return Result<IImmutableList<Printer>>.Failure(result.Message, result.StatusCode, result.Problem);
        }

        return Result<IImmutableList<Printer>>.Success(result.Value.ToPrinterList().ToImmutableArray());
    }

    public async ValueTask<Result<EmptyRecord>> UpdatePrinter(Printer printer)
    {
        var updatePrinterDto = printer.ToUpdatePrinterDto();

        var result = await httpService.PutAsync("api/printer/update", updatePrinterDto, JsonContext.Default.EmptyRecord);

        if (!result.Successful)
        {
            return Result<EmptyRecord>.Failure(result.Message, result.StatusCode, result.Problem);
        }

        return result;
    }

    public async Task<Result<EmptyRecord>> DeletePrinter(Guid printerId, CancellationToken cancellationToken)
    {
        return await httpService.DeleteAsync($"api/printer/delete/{printerId}", cancellationToken);
    }
}
