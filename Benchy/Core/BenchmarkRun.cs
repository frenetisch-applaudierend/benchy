using Benchy.Infrastructure;

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
        DirectoryInfo temporaryDirectory,
        string name,
        IEnumerable<string> benchmarks
    )
    {
        var benchmarkProjects = benchmarks
            .Select(benchmark => BenchmarkProject.FromName(benchmark, sourceDirectory))
            .ToList();

        var outputDirectory = temporaryDirectory.CreateSubdirectory("out");

        return new BenchmarkRun(name, outputDirectory, benchmarkProjects);
    }

    public void Prepare(bool verbose)
    {
        foreach (var project in BenchmarkProjects)
        {
            project.Build(verbose);
        }
    }

    public BenchmarkResult Run(bool verbose)
    {
        Output.Info($"Running benchmarks for {Name}");

        foreach (var project in BenchmarkProjects)
        {
            project.Run(OutputDirectory, verbose);
        }

        return new BenchmarkResult(
            this,
            [.. BenchmarkReport.LoadReports(OutputDirectory.SubDirectory("results"))]
        );
    }
}
