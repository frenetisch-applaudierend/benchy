namespace Benchy.Output;

public interface IOutputWriter
{
    void Write(FormattedText text);
    void WriteLine(FormattedText text);
}
