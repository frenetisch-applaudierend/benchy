namespace Benchy.Output;

public abstract class FormattedText
{
    public abstract void WriteTo(TextWriter writer, bool interactive);

    public static implicit operator FormattedText(string text) => new PlainText(text);

    public static FormattedText operator +(FormattedText left, FormattedText right) =>
        new ConcatenatedText(left, right);

    public static FormattedText Decor(string text) => new DecorationText(text);

    public static FormattedText Em(FormattedText text) => new ColoredText(text, ConsoleColor.Cyan);
}

file sealed class PlainText(string text) : FormattedText
{
    public override void WriteTo(TextWriter writer, bool interactive)
    {
        writer.Write(text);
    }
}

file sealed class DecorationText(string text) : FormattedText
{
    public override void WriteTo(TextWriter writer, bool interactive)
    {
        if (interactive)
        {
            writer.Write(text);
        }
    }
}

file sealed class ColoredText(FormattedText text, ConsoleColor color) : FormattedText
{
    public override void WriteTo(TextWriter writer, bool interactive)
    {
        var originalColor = Console.ForegroundColor;

        if (interactive)
            Console.ForegroundColor = color;

        text.WriteTo(writer, interactive);

        if (interactive)
            Console.ForegroundColor = originalColor;
    }
}

file sealed class ConcatenatedText(FormattedText left, FormattedText right) : FormattedText
{
    public override void WriteTo(TextWriter writer, bool interactive)
    {
        left.WriteTo(writer, interactive);
        right.WriteTo(writer, interactive);
    }
}
