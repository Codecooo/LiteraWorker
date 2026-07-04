using XenoAtom.CommandLine;

namespace Litera.Cli.Core.Commands;

public abstract class PrintCommand
{
    const string whiteSpace = "";

    public Command Build()
    {
        var file = string.Empty;
        var copies = 1;
        var quality = "normal";
        var colorMode = true;
        var route = "online";

        return new Command("print", "Send a print request to the server")
        {
            new HelpOption(),
            whiteSpace,
            "Options:",
            {"f|file=", "Provide the file location to print (PDF, image)", v => file = v},
            {"q|quality=", "Provide the quality setting to print (draft, normal, high)", v => quality = v},
            {"n|copies=", "How many copies to print (default: 1)", v => copies = int.TryParse(v, out var n) ? n : 1},
            {"c|color=", "Specify the color mode (color, grayscale)", v => colorMode = !bool.TryParse(v, out var color) || color},
            {"r|route=", "Where the print take place (online, local) default online", v => route = v}
        };
    }
}
