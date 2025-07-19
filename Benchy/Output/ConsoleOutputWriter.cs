namespace Benchy.Output;

public class ConsoleOutputWriter(bool interactive) : IOutputWriter
{
    public void Write(FormattedText text)
    {
        text.WriteTo(Console.Out, interactive);
    }

    public void WriteLine(FormattedText text)
    {
        text.WriteTo(Console.Out, interactive);
        Console.Out.WriteLine();
    }
}
