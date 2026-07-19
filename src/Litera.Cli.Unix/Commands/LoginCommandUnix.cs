using Litera.Cli.Core.Commands;
using Litera.Cli.Core.Helpers;
using LiteraWorker.Core.DTO;
using LiteraWorker.Core.Services.Auth;
using XenoAtom.CommandLine;

namespace Litera.Cli.Unix.Commands;

public class LoginCommandUnix(IAuthProvider authProvider) : LoginCommand
{
    public override async ValueTask<int> Execute(CommandRunContext ctx, string email)
    {
        if (string.IsNullOrEmpty(email))
        {
            ctx.Out.Write("Enter your email: ");
            email = Console.ReadLine() ?? string.Empty;
        }

        ctx.Out.Write("Enter your password: ");
        var password = PasswordInput.SecurePassword();

        ctx.Out.WriteLine($"Logging in {email}...");
        var loginRequest = new LoginRequestDto(email, password);

        try
        {
            var result = await authProvider.Login(loginRequest);
            if (!result.Successful)
            {
                ctx.Out.WriteLine("Login failed!");
                var error = result.Problem?.Detail;
                ErrorHelpers.OutputError(ctx, error ?? "Unexpected error happened");
                return 1;
            }
        }
        catch (HttpRequestException ex)
        {
            ErrorHelpers.OutputError(ctx, $"Cannot connect to server: {ex.Message}");
            return 1;
        }

        ctx.Out.WriteLine("Login successful");
        ctx.Out.WriteLine($"Welcome {email}");
        return 0;
    }
}
