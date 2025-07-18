namespace Benchy.Configuration;

public class ResolvedConfig
{
    public bool Verbose { get; set; }
    public DirectoryInfo? OutputDirectory { get; set; }
    public string[] OutputStyle { get; set; } = [];
    public string[] Benchmarks { get; set; } = [];

    // Interactive-specific
    public bool NoDelete { get; set; }

    public static ResolvedConfig CreateDefault(bool isInteractiveMode)
    {
        return new ResolvedConfig
        {
            Verbose = false,
            OutputDirectory = null,
            OutputStyle = isInteractiveMode ? ["console"] : ["json", "markdown"],
            Benchmarks = [],
            NoDelete = false,
        };
    }
}
