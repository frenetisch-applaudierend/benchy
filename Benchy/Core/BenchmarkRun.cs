using Benchy.Infrastructure;
using Benchy.Output;

namespace Benchy.Core;

public sealed class BenchmarkRun
{
    public string Name { get; }
    public DirectoryInfo OutputDirectory { get; }

    public IReadOnlyList<BenchmarkProject> BenchmarkProjects { get; }

    private BenchmarkRun(
        string name,
        DirectoryInfo outputDirectory,
        IReadOnlyList<BenchmarkProject> benchmarkProjects
    )
    {
        Name = name;
        OutputDirectory = outputDirectory;
        BenchmarkProjects = benchmarkProjects;
    }

    public static BenchmarkRun FromSourcePath(
        DirectoryInfo sourceDirectory,
        DirectoryInfo outputDirectory,
        string name,
        IEnumerable<string> benchmarks
    )
    {
        var benchmarkProjects = benchmarks
            .Select(benchmark => BenchmarkProject.FromName(benchmark, sourceDirectory))
            .ToList();

        return new BenchmarkRun(name, outputDirectory, benchmarkProjects);
    }

    public void Prepare(bool verbose)
    {
        foreach (var project in BenchmarkProjects)
        {
            project.Build(verbose);
        }
    }

    public BenchmarkRunResult Run(bool verbose)
    {
        CliOutput.Info($"Running benchmarks for {Name}");

        foreach (var project in BenchmarkProjects)
        {
            project.Run(OutputDirectory, verbose);
        }

        return new BenchmarkRunResult(
            this,
            [.. BenchmarkReport.LoadReports(OutputDirectory.Subdirectory("results"))]
        );
    }
}
