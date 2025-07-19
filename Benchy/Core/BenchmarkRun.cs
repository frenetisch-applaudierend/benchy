using Benchy.Infrastructure;
using Benchy.Output;
using static Benchy.Output.FormattedText;

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
            CliOutput.Info(
                $"{Decor("🛠️  ")}{Dim("Building benchmark project:")} {Em(project.Name)} for {Em(Name)}",
                indent: 1
            );
            project.Build(verbose);
        }
    }

    public BenchmarkRunResult Run(bool verbose)
    {
        foreach (var project in BenchmarkProjects)
        {
            CliOutput.Info(
                $"{Decor("🏁 ")}Running benchmark project {Em(project.Name)} for {Em(Name)}",
                indent: 1
            );
            project.Run(OutputDirectory, verbose);
        }

        return new BenchmarkRunResult(
            this,
            [.. BenchmarkReport.LoadReports(OutputDirectory.Subdirectory("results"))]
        );
    }
}
