using Benchy.Infrastructure;

namespace Benchy.Core;

public sealed class BenchmarkRun
{
    public DirectoryInfo OutputDirectory { get; }

    public IReadOnlyList<BenchmarkProject> BenchmarkProjects { get; }

    private BenchmarkRun(
        DirectoryInfo outputDirectory,
        IReadOnlyList<BenchmarkProject> benchmarkProjects
    )
    {
        OutputDirectory = outputDirectory;
        BenchmarkProjects = benchmarkProjects;
    }

    public static BenchmarkRun FromSourcePath(
        DirectoryInfo sourceDirectory,
        DirectoryInfo temporaryDirectory,
        IEnumerable<string> benchmarks
    )
    {
        var benchmarkProjects = benchmarks
            .Select(benchmark => BenchmarkProject.FromName(benchmark, sourceDirectory))
            .ToList();

        var outputDirectory = temporaryDirectory.CreateSubdirectory("out");

        return new BenchmarkRun(outputDirectory, benchmarkProjects);
    }

    public static BenchmarkRun FromCommitReference(
        GitRepository sourceRepository,
        DirectoryInfo temporaryDirectory,
        string commitReference,
        IEnumerable<string> benchmarks
    )
    {
        throw new NotImplementedException("This method is not implemented yet.");
    }
}
