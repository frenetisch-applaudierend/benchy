namespace Benchy.Output;

public abstract class FormattedText
{
    public abstract void WriteTo(TextWriter writer, bool interactive);

    public static implicit operator FormattedText(string text) => new PlainText(text);

    public static FormattedText operator +(FormattedText left, FormattedText right) =>
        new ConcatenatedText(left, right);

    public static FormattedText Decor(FormattedText text) => new DecorationText(text, null);

    public static FormattedText Decor(FormattedText text, string alternative) =>
        new DecorationText(text, alternative);

    public static FormattedText Decor(FormattedTextInterpolatedStringHandler handler) =>
        new DecorationText(handler.ToFormattedText(), null);

    public static FormattedText Decor(
        FormattedTextInterpolatedStringHandler handler,
        string alternative
    ) => new DecorationText(handler.ToFormattedText(), alternative);

    public static FormattedText Colored(FormattedText text, ConsoleColor color) =>
        new ColoredText(text, color);

    public static FormattedText Em(FormattedText text) => new ColoredText(text, ConsoleColor.Cyan);

    public static FormattedText Dim(FormattedText text) => new ColoredText(text, ConsoleColor.Gray);

    public static FormattedText Join(params FormattedText[] parts) => new ConcatenatedText(parts);
}

file sealed class PlainText(string text) : FormattedText
{
    public override void WriteTo(TextWriter writer, bool interactive)
    {
        writer.Write(text);
    }
}

file sealed class DecorationText(FormattedText text, string? alternative) : FormattedText
{
    public override void WriteTo(TextWriter writer, bool interactive)
    {
        if (interactive)
        {
            text.WriteTo(writer, interactive);
        }
        else if (alternative is not null)
        {
            writer.Write(alternative);
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

file sealed class ConcatenatedText : FormattedText
{
    private readonly FormattedText[] _parts;

    public ConcatenatedText(FormattedText left, FormattedText right)
    {
        _parts = [left, right];
    }

    public ConcatenatedText(FormattedText[] parts)
    {
        _parts = parts;
    }

    public override void WriteTo(TextWriter writer, bool interactive)
    {
        foreach (var part in _parts)
        {
            part.WriteTo(writer, interactive);
        }
    }
}
