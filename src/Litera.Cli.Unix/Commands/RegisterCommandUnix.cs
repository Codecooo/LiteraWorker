using Litera.Cli.Core.Commands;
using Litera.Cli.Core.Helpers;
using LiteraWorker.Core.DTO;
using LiteraWorker.Core.Services.Auth;
using XenoAtom.CommandLine;

namespace Litera.Cli.Unix.Commands;

public class RegisterCommandUnix(IAuthProvider authProvider) : RegisterCommand
{
    public override async ValueTask<int> Execute(CommandRunContext ctx, string email, string username)
    {
        if (string.IsNullOrEmpty(email))
        {
            ctx.Out.Write("Enter your email: ");
            email = Console.ReadLine() ?? string.Empty;
        }
        if (string.IsNullOrEmpty(username))
        {
            ctx.Out.Write("Enter your username: ");
            username = Console.ReadLine() ?? string.Empty;
        }

        ctx.Out.Write("Enter your password: ");
        var password = PasswordInput.SecurePassword();

        ctx.Out.WriteLine($"Logging in {email}...");
        var userDto = new UserDto
        {
            Name = username,
            Email = email, 
            Password = password
        };
        
        try
        {
            ctx.Out.WriteLine($"Registering {username}...");
            var result = await authProvider.RegisterUser(userDto);

            if (!result.Successful)
            {
                ctx.Out.WriteLine("Register failed");
                ErrorHelpers.OutputError(ctx, result.Problem!.Detail!);
                return 1;
            }
        }
        catch (HttpRequestException ex)
        {
            ErrorHelpers.OutputError(ctx, $"Cannot connect to server: {ex.Message}");
            return 1;
        }

        ctx.Out.WriteLine("Register successful");
        ctx.Out.WriteLine($"Welcome {email}");
        return 0;
    }
}