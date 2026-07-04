using XenoAtom.CommandLine;

namespace Litera.Cli.Core.Commands;

public abstract class LoginCommand
{
    const string whiteSpace = "";

    public Command Build()
    {
        var email = string.Empty;

        return new Command("login", "Login to the CLI")
        {
            new HelpOption(),
            whiteSpace,
            "Options:",
            {"e|email=", "Provide the email for the account to login", v => email = v ?? string.Empty},

            (ctx, whiteSpace) => Execute(ctx, email)
        };
    }

    /// <summary>
    /// Abstract for login command implementation depending on platforms
    /// </summary>
    /// <param name="ctx"></param>
    /// <param name="email"></param>
    /// <returns></returns>
    public abstract ValueTask<int> Execute(CommandRunContext ctx, string email);
}
