namespace Benchy.Configuration;

public class ConfigFromFile : ConfigValues
{
    public ConfigValues Interactive { get; set; } = new();
    public ConfigValues Ci { get; set; } = new();
}

public class ConfigValues
{
    public bool? Verbose { get; set; }
    public string? OutputDirectory { get; set; }
    public string[]? OutputStyle { get; set; }
    public string[]? Benchmarks { get; set; }
    public bool? NoDelete { get; set; }
    public double? SignificanceThreshold { get; set; }
}
