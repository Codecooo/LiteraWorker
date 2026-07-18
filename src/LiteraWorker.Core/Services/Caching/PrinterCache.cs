using System.Collections.Immutable;
using LiteraWorker.Core.Helpers;
using LiteraWorker.Core.Models;
using LiteraWorker.Core.Services.ApiService;
using LiteraWorker.Core.Services.Printing;
using Microsoft.Extensions.Logging;

namespace LiteraWorker.Core.Services.Caching;

public sealed class PrinterCache(
    IUserCache userCache,
    ILogger<PrinterCache> logger,
    PrinterService printerService,
    IPrintOps printOps) : IPrinterCache
{
    private readonly IUserCache _userCache = userCache;
    private readonly ILogger<PrinterCache> _logger = logger;
    private readonly PrinterService _printerService = printerService;
    private readonly IPrintOps _printOps = printOps;
    private readonly static string _cacheDir = Path.Combine(Environment.GetEnvironmentVariable("CACHE_DIRECTORY") ?? "");
    private readonly string _cachePath = LocalCacheSet.InitializeCachePath(Path.Combine(_cacheDir, "printers.json"));

    public async ValueTask<Result<Printer>> GetPrinterDetails(Guid id, CancellationToken token)
    {
        var cached = await GetCachedPrinters(token);

        var match = cached.Value!.FirstOrDefault(p => p?.Id == id);
        if (match != null)
        {
            return Result<Printer>.Success(match);
        }

        await EnsureOnlineAsync();
        var all = await RefreshFromServerAsync(token);

        if (!all.Successful)
        {
            return Result<Printer>.Failure(all.Message, all.StatusCode, all.Problem);
        }
        var filteredResult = all.Value!.FirstOrDefault(p => p?.Id == id);
        return Result<Printer>.Success(filteredResult!);
    }

    public async ValueTask<Result<IImmutableList<Printer>>> GetPrinters(CancellationToken token)
    {
        var cached = await GetCachedPrinters(token);
        if (cached.Successful && cached.Value == null)
        {
            // empty list
            return Result<IImmutableList<Printer>>.Success(cached.Value!);
        };

        await EnsureOnlineAsync();
        var refreshed = await RefreshFromServerAsync(token);
        return refreshed;
    }

    private async Task EnsureOnlineAsync()
    {
        if (!await NetworkHelpers.InternetAvailable())
        {
            _logger.LogWarning("App is offline and cannot connect to the API.");
            throw new OfflineException("No internet connection");
        }
    }

    private async ValueTask<Result<IImmutableList<Printer>>> RefreshFromServerAsync(CancellationToken token)
    {
        var currentUser = await _userCache.GetCurrentUser(token);
        var printerResult = await _printerService.GetPrinters(currentUser.Value!.Id, token);

        var printers = printerResult.Value;

        if (printers == null || !printers.Any())
        {
            _logger.LogWarning("No printers found for the current user from the API.");
            return Result<IImmutableList<Printer>>.Success(ImmutableArray<Printer>.Empty);
        }

        await LocalCacheSet.WriteCache(printers, _cachePath);
        return printerResult;
    }

    private async ValueTask<Result<IImmutableList<Printer>>> GetCachedPrinters(
    CancellationToken token)
    {
        // Try to read a fresh cache first
        if (await LocalCacheSet.IsCacheValid(_cachePath, TimeSpan.FromMinutes(5)))
        {
            var cached = await LocalCacheSet.ReadCache<IImmutableList<Printer>>(_cachePath);
            return Result<IImmutableList<Printer>>.Success(
                cached ?? ImmutableArray<Printer>.Empty);
        }

        // Pull the list of printers that the OS reports
        var osPrinters = await _printOps.GetPrintersInfo(token);

        // Pull the list we already have registered on the server
        Result<IImmutableList<Printer>> refreshResult = new();

        try
        {
            refreshResult = await RefreshFromServerAsync(token);
        }
        catch (Exception)
        {
            // Ignore any exception so we can continue 
        }
        var apiPrinters = refreshResult.Value ?? ImmutableArray<Printer>.Empty;

        //  Remove any duplicates inside the OS list (e.g. two
        //   entries that refer to the same physical printer).
        //   We use the custom comparer that treats Name+Uri+DeviceId as the
        //   identity of a printer.
        var distinctOsPrinters = osPrinters
            .Distinct(new PrinterIdentityComparer())
            .ToImmutableArray();

        // Find printers that are present on the OS but not yet
        // registered on the server
        var newPrinters = distinctOsPrinters
            .Except(apiPrinters, new PrinterIdentityComparer())
            .ToImmutableArray();

        // If there are any truly new printers, register them
        if (newPrinters.Any())
        {
            await _printerService.RegisterPrinters(newPrinters);
        }

        // Pull the final, up‑to‑date list from the server and cache it
        var finalPrinters = await RefreshFromServerAsync(token);
        if (!finalPrinters.Successful)
        {
            return finalPrinters;
        }

        await LocalCacheSet.WriteCache(finalPrinters.Value!, _cachePath);
        return finalPrinters;
    }

    public async ValueTask ClearCache(CancellationToken token)
    {
        await LocalCacheSet.ClearCache(_cachePath);
    }
}


// Custom exception for clarity
public sealed class OfflineException(string message) : Exception(message);