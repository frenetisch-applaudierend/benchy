namespace Benchy.Infrastructure;

public static class Output
{
    public static bool EnableVerbose { get; set; } = false;

    public static void Info(string message) => Console.Out.WriteLine(message);

    public static void Info(string message, int indent) => Console.Out.WriteLine(new string(' ', indent) + message);

    public static void Verbose(string message)
    {
        if (EnableVerbose)
        {
            Console.Out.WriteLine(message);
        }
    }

    public static void Verbose(string message, int indent)
    {
        if (EnableVerbose)
        {
            Console.Out.WriteLine(new string(' ', indent) + message);
        }
    }

    public static void Error(string message) => Console.Error.WriteLine(message);
}
