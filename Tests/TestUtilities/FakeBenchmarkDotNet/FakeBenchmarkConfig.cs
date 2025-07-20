namespace FakeBenchmarkDotNet;

public record FakeBenchmarkConfig
{
    public List<OutputFile> OutputFiles { get; init; } = new();
    public bool ShouldFail { get; init; } = false;
    public TimeSpan ExecutionDelay { get; init; } = TimeSpan.Zero;
    public string BenchmarkName { get; init; } = "TestBenchmark";
    
    public static FakeBenchmarkConfig CreateDefault() => new();
    
    public static FakeBenchmarkConfig WithJsonOutput(string fileName, string jsonContent)
    {
        return new FakeBenchmarkConfig
        {
            OutputFiles = new List<OutputFile>
            {
                new OutputFile(fileName, JsonContent: jsonContent)
            }
        };
    }
    
    public static FakeBenchmarkConfig WithFileOutput(string fileName, string sourcePath)
    {
        return new FakeBenchmarkConfig
        {
            OutputFiles = new List<OutputFile>
            {
                new OutputFile(fileName, SourcePath: sourcePath)
            }
        };
    }
    
    public FakeBenchmarkConfig WithAdditionalOutput(string fileName, string? jsonContent = null, string? sourcePath = null)
    {
        var newOutputFiles = new List<OutputFile>(OutputFiles)
        {
            new OutputFile(fileName, jsonContent, sourcePath)
        };
        
        return this with { OutputFiles = newOutputFiles };
    }
    
    public FakeBenchmarkConfig WithFailure(bool shouldFail = true)
    {
        return this with { ShouldFail = shouldFail };
    }
    
    public FakeBenchmarkConfig WithDelay(TimeSpan delay)
    {
        return this with { ExecutionDelay = delay };
    }
}

public record OutputFile(
    string FileName,
    string? JsonContent = null,
    string? SourcePath = null
)
{
    public bool HasValidContent => !string.IsNullOrEmpty(JsonContent) || 
                                   (!string.IsNullOrEmpty(SourcePath) && File.Exists(SourcePath));
}