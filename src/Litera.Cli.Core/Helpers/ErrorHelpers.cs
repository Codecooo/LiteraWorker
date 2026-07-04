using XenoAtom.CommandLine;

namespace Litera.Cli.Core.Helpers;

public static class ErrorHelpers
{
    public static void OutputError(CommandRunContext ctx, string message = "Something wrong")
    {
        Console.ForegroundColor = ConsoleColor.Red;
        ctx.Error.WriteLine(message);
        Console.ResetColor();
    }
}
