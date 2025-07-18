using Benchy.Core;
using Benchy.Infrastructure;
using Benchy.Infrastructure.Reporting;

namespace Benchy.Cli;

public static class CiHandler
{
    public static void Handle(
        bool verbose,
        DirectoryInfo? providedOutputDirectory,
        string[] outputStyles,
        string[] benchmarks,
        DirectoryInfo baselineDirectory,
        DirectoryInfo targetDirectory
    )
    {
        Output.EnableVerbose = verbose;

        if (benchmarks.Length == 0)
        {
            throw new ArgumentException("At least one benchmark must be specified.");
        }

        using var temporaryDirectory = TemporaryDirectory.CreateNew(keep: true);
        Output.Verbose($"Temporary directory for comparison: {temporaryDirectory.FullName}");

        var outputDirectory =
            providedOutputDirectory ?? temporaryDirectory.CreateSubdirectory("out");

        var baselineRun = BenchmarkRun.FromSourcePath(
            sourceDirectory: baselineDirectory,
            outputDirectory: outputDirectory.CreateSubdirectory("baseline"),
            name: "baseline",
            benchmarks: benchmarks
        );

        var targetRun = BenchmarkRun.FromSourcePath(
            sourceDirectory: targetDirectory,
            outputDirectory: outputDirectory.CreateSubdirectory("target"),
            name: "target",
            benchmarks: benchmarks
        );

        var results = BenchmarkComparer.CompareBenchmarks(baselineRun, targetRun, verbose);

        var reporter = Reporting.CreateReporter(
            outputStyles,
            outputDirectory,
            Console.Out,
            useColors: false,
            isInteractiveMode: false
        );
        reporter.GenerateReport(results);
    }
}
