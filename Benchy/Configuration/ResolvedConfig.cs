namespace Benchy.Configuration;

public class ResolvedConfig
{
    public required bool Verbose { get; init; }
    public required DirectoryInfo OutputDirectory { get; init; }
    public required string[] OutputStyle { get; init; }
    public required string[] Benchmarks { get; init; }
    public required bool NoDelete { get; init; }
}
