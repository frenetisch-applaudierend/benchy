namespace Benchy.Configuration;

public class BenchyConfig
{
    public GlobalConfig Global { get; set; } = new();
    public InteractiveConfig Interactive { get; set; } = new();
    public CiConfig Ci { get; set; } = new();
}

public class GlobalConfig
{
    public bool? Verbose { get; set; }
    public string? OutputDirectory { get; set; }
    public string[]? OutputStyle { get; set; }
    public string[]? Benchmarks { get; set; }
}

public class InteractiveConfig
{
    public bool? Verbose { get; set; }
    public string? OutputDirectory { get; set; }
    public string[]? OutputStyle { get; set; }
    public string[]? Benchmarks { get; set; }
    public bool? NoDelete { get; set; }
}

public class CiConfig
{
    public bool? Verbose { get; set; }
    public string? OutputDirectory { get; set; }
    public string[]? OutputStyle { get; set; }
    public string[]? Benchmarks { get; set; }
}
