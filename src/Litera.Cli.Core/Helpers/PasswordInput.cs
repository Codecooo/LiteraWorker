namespace Litera.Cli.Core.Helpers;

public static class PasswordInput
{
    /// <summary>
    /// A helper for providing secure censored password input
    /// </summary>
    /// <returns></returns>
    public static string SecurePassword()
    {
        var pass = string.Empty;
        ConsoleKey key;
        do
        {
            var keyInfo = Console.ReadKey(intercept: true);
            key = keyInfo.Key;

            if (key == ConsoleKey.Backspace && pass.Length > 0)
            {
                Console.Write("\b \b");
                pass = pass[0..^1];
            }
            else if (!char.IsControl(keyInfo.KeyChar))
            {
                pass += keyInfo.KeyChar;
            }
        } while (key != ConsoleKey.Enter);
        Console.WriteLine();

        return pass;
    }
}
