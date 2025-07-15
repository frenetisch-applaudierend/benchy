using Benchy.Core;
using Benchy.Infrastructure;

namespace Benchy.Cli;

public static class CiHandler
{
    public static void Handle(
        bool verbose,
        string[] benchmarks,
        DirectoryInfo baselineDirectory,
        DirectoryInfo targetDirectory
    )
    {
        Output.EnableVerbose = verbose;

        using var temporaryDirectory = TemporaryDirectory.CreateNew(keep: true);
        Output.Verbose($"Temporary directory for comparison: {temporaryDirectory.FullName}");

        var baselineTemporaryDirectory = temporaryDirectory.CreateSubDirectory("baseline");
        var baselineRun = BenchmarkRun.FromSourcePath(
            sourceDirectory: baselineDirectory,
            temporaryDirectory: baselineTemporaryDirectory,
            name: "baseline",
            benchmarks: benchmarks
        );

        var targetTemporaryDirectory = temporaryDirectory.CreateSubDirectory("target");
        var targetRun = BenchmarkRun.FromSourcePath(
            sourceDirectory: targetDirectory,
            temporaryDirectory: targetTemporaryDirectory,
            name: "target",
            benchmarks: benchmarks
        );

        var results = BenchmarkComparer.CompareBenchmarks(baselineRun, targetRun, verbose);

        _ = results; // TODO
    }
}
