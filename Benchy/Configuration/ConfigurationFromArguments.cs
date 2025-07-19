namespace Benchy.Configuration;

public class ConfigFromArguments
{
    public bool? Verbose { get; init; }
    public string? OutputDirectory { get; init; }
    public string[]? OutputStyle { get; init; }
    public string[]? Benchmarks { get; init; }
    public bool? NoDelete { get; init; }
}
