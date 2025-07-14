namespace Benchy.Core;

public sealed class BenchmarkRun
{
    public string SourcePath { get; }
    public IReadOnlyList<BenchmarkProject> BenchmarkProjects { get; }
}

public sealed record BenchmarkProject(string Name);
