using System.Runtime.CompilerServices;

namespace Benchy.Output;

[InterpolatedStringHandler]
public ref struct FormattedTextInterpolatedStringHandler
{
    private readonly List<FormattedText> _parts;

    public FormattedTextInterpolatedStringHandler(int literalLength, int formattedCount)
    {
        _parts = new List<FormattedText>(formattedCount + 1);
    }

    public readonly void AppendLiteral(string s)
    {
        if (!string.IsNullOrEmpty(s))
        {
            _parts.Add(new PlainText(s));
        }
    }

    public readonly void AppendFormatted<T>(T value)
    {
        if (value is FormattedText formattedText)
        {
            _parts.Add(formattedText);
        }
        else if (value is not null)
        {
            _parts.Add(new PlainText(value.ToString() ?? string.Empty));
        }
    }

    public readonly void AppendFormatted<T>(T value, string? format)
    {
        if (value is FormattedText formattedText)
        {
            _parts.Add(formattedText);
        }
        else if (value is IFormattable formattable)
        {
            _parts.Add(new PlainText(formattable.ToString(format, null)));
        }
        else if (value is not null)
        {
            _parts.Add(new PlainText(value.ToString() ?? string.Empty));
        }
    }

    public readonly FormattedText ToFormattedText()
    {
        return _parts.Count switch
        {
            0 => new PlainText(string.Empty),
            1 => _parts[0],
            _ => new ConcatenatedText([.. _parts]),
        };
    }
}

public abstract class FormattedText
{
    public abstract void WriteTo(TextWriter writer, bool interactive);

    public static implicit operator FormattedText(string text) => new PlainText(text);

    public static implicit operator FormattedText(FormattedTextInterpolatedStringHandler handler) =>
        handler.ToFormattedText();

    public static FormattedText operator +(FormattedText left, FormattedText right) =>
        new ConcatenatedText(left, right);

    public static FormattedText Decor(string text) => new DecorationText(text);

    public static FormattedText Em(FormattedText text) => new ColoredText(text, ConsoleColor.Cyan);

    public static FormattedText Em(string text) =>
        new ColoredText(new PlainText(text), ConsoleColor.Cyan);
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
