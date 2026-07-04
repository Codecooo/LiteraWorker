using LiteraWorker.Core.Services.Auth;
using XenoAtom.CommandLine;

namespace Litera.Cli.Core.Commands;

public class StatusCommand(IAuthProvider authProvider)
{
    const string whiteSpace = "";

    public Command Build()
    {
        return new Command("status", "Check the current status of the app")
        {
            new HelpOption(),
            whiteSpace,
            (ctx, whitespace) => Execute(ctx) 
        };
    }

    private async ValueTask<int> Execute(CommandRunContext ctx)
    {
        var authenticated = await authProvider.IsAuthenticated();
        if (!authenticated)
        {
            ctx.Out.WriteLine("The current CLI session is not authenticated. Please login first using 'litera login'");
            return 1;
        }

        ctx.Out.WriteLine("The CLI session is authenticated");
        return 0;
    }
}
