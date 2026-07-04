using LiteraWorker.Core.Services.Caching;
using XenoAtom.CommandLine;

namespace Litera.Cli.Core.Commands;

public class DiscoverCommand(IPrinterCache printerCache)
{
    public Command Build()
    {
        return new Command("discover", "Discover to recognize printers")
        {
            new HelpOption(help: "Discover to recognize printers"),
            (ctx, whitespace) => Execute(ctx) 
        };
    }

    private async ValueTask<int> Execute(CommandRunContext ctx)
    {
        ctx.Out.WriteLine("Trying to discover any printer in the device...");
        var result = await printerCache.GetPrinters(CancellationToken.None);
        ctx.Out.WriteLine($"Found {result.Value?.Count} printers!");
        return 0;
    }
}
