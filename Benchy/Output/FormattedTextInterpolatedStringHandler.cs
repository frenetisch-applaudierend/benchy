using System.Runtime.CompilerServices;

namespace Benchy.Output;

[InterpolatedStringHandler]
public readonly ref struct FormattedTextInterpolatedStringHandler
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
            _parts.Add(s);
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
            _parts.Add(value.ToString() ?? string.Empty);
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
            _parts.Add(formattable.ToString(format, null));
        }
        else if (value is not null)
        {
            _parts.Add(value.ToString() ?? string.Empty);
        }
    }

    public readonly FormattedText ToFormattedText()
    {
        return _parts.Count switch
        {
            0 => string.Empty,
            1 => _parts[0],
            _ => FormattedText.Join([.. _parts]),
        };
    }
}
