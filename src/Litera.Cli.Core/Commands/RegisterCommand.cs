using XenoAtom.CommandLine;

namespace Litera.Cli.Core.Commands;

public abstract class RegisterCommand
{
    const string whiteSpace = "";

    public Command Build()
    {
        var email = string.Empty;
        var username = string.Empty;

        return new Command("register", "Register to the CLI")
        {
            new HelpOption(),
            whiteSpace,
            "Options:",
            {"e|email=", "Provide the email for the account to register", v => email = v ?? string.Empty},
            {"u|username=", "Provide the username for the account to register", v => username = v ?? string.Empty},

            (ctx, whiteSpace) => Execute(ctx, email, username)
        };
    }

    /// <summary>
    /// Abstract for login command implementation depending on platforms
    /// </summary>
    /// <param name="ctx"></param>
    /// <param name="email"></param>
    /// <returns></returns>
    public abstract ValueTask<int> Execute(CommandRunContext ctx, string email, string username);
}
