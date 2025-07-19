using System;

namespace Benchy.Output;

public static class CliOutput
{
    public static bool EnableVerbose { get; set; } = false;

    public static IOutputWriter Writer { get; set; } = new ConsoleOutputWriter(interactive: false);

    public static void Info(FormattedText message) => Writer.WriteLine(message);

    public static void Info(FormattedText message, int indent) =>
        Writer.WriteLine(new string(' ', indent) + message);

    public static void Verbose(FormattedText message)
    {
        if (EnableVerbose)
        {
            Writer.WriteLine(message);
        }
    }

    public static void Verbose(FormattedText message, int indent)
    {
        if (EnableVerbose)
        {
            Writer.WriteLine(new string(' ', indent) + message);
        }
    }

    public static void Error(FormattedText message) => Writer.WriteLine(message);
}
