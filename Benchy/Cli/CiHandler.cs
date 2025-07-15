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
        Console.WriteLine("=== CI MODE ===");
        Console.WriteLine($"Baseline directory: {baselineDirectory.FullName}");
        Console.WriteLine($"Target directory: {targetDirectory.FullName}");
        Console.WriteLine($"Benchmarks: [{string.Join(", ", benchmarks)}]");
        Console.WriteLine($"Verbose: {verbose}");

        var baselineTemporaryDirectory = Directories.CreateTemporaryDirectory("ci-baseline");
        var baselineRun = BenchmarkRun.FromSourcePath(
            baselineDirectory,
            baselineTemporaryDirectory,
            benchmarks
        );

        var targetTemporaryDirectory = Directories.CreateTemporaryDirectory("ci-target");
        var targetRun = BenchmarkRun.FromSourcePath(
            targetDirectory,
            targetTemporaryDirectory,
            benchmarks
        );

        var results = BenchmarkComparer.CompareBenchmarks(baselineRun, targetRun, verbose);

        Console.WriteLine("=== COMPARISON RESULTS ===");
        Console.WriteLine(results.ToString());
    }
}
