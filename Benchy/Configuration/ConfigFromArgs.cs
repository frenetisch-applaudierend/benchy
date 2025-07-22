using System;

namespace Benchy.Configuration;

public class ConfigFromArgs
{
    public required bool? Verbose { get; init; }
    public required string? OutputDirectory { get; init; }
    public required string[]? OutputStyle { get; init; }
    public required string[]? Benchmarks { get; init; }
    public required bool? NoDelete { get; init; }
    public required double? SignificanceThreshold { get; init; }
}
