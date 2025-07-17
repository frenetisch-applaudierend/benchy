using System.Numerics;
using Benchy.Core;
using Benchy.Infrastructure;

namespace Benchy.Cli;

public static class InteractiveHandler
{
    public static void Handle(
        bool verbose,
        DirectoryInfo? providedOutputDirectory,
        string[] benchmarks,
        DirectoryInfo? repositoryPath,
        bool noDelete,
        string baselineRef,
        string? targetRef
    )
    {
        Output.EnableVerbose = verbose;

        using var temporaryDirectory = TemporaryDirectory.CreateNew(keep: noDelete);
        Output.Verbose($"Temporary directory for comparison: {temporaryDirectory.FullName}");

        var outputDirectory =
            providedOutputDirectory ?? temporaryDirectory.CreateSubdirectory("out");
        var sourceDirectory = temporaryDirectory.CreateSubdirectory("src");

        var repository = FindRepository(repositoryPath);

        var baselineRun = CheckoutRun(
            repository: repository,
            label: "baseline",
            reference: baselineRef,
            sourceDirectory: sourceDirectory,
            outputDirectory: outputDirectory,
            benchmarks: benchmarks
        );

        var targetRun = CheckoutRun(
            repository: repository,
            label: "target",
            reference: targetRef,
            sourceDirectory: sourceDirectory,
            outputDirectory: outputDirectory,
            benchmarks: benchmarks
        );

        var results = BenchmarkComparer.CompareBenchmarks(baselineRun, targetRun, verbose);

        PrintBenchmarkComparison(results);

        if (noDelete)
        {
            Output.Info(
                $"Skipping cleanup of temporary directories at {temporaryDirectory.FullName}"
            );
        }
    }

    private static GitRepository FindRepository(DirectoryInfo? repositoryPath)
    {
        if (repositoryPath is not null)
        {
            return GitRepository.Open(repositoryPath.FullName);
        }

        // Auto-detect repository from current directory
        return GitRepository.Open(Directory.GetCurrentDirectory());
    }

    private static BenchmarkRun CheckoutRun(
        GitRepository repository,
        string label,
        string? reference,
        DirectoryInfo sourceDirectory,
        DirectoryInfo outputDirectory,
        string[] benchmarks
    )
    {
        var runSourceDirectory = CheckoutReference(
            repository: repository,
            reference: reference,
            sourceDirectory: sourceDirectory.CreateSubdirectory(label)
        );

        return BenchmarkRun.FromSourcePath(
            sourceDirectory: runSourceDirectory,
            outputDirectory: outputDirectory.CreateSubdirectory(label),
            name: $"{label} ({reference ?? "working copy"})",
            benchmarks: benchmarks
        );
    }

    private static DirectoryInfo CheckoutReference(
        GitRepository repository,
        string? reference,
        DirectoryInfo sourceDirectory
    )
    {
        if (string.IsNullOrEmpty(reference))
        {
            throw new NotImplementedException("No target reference is not implemented yet.");
        }

        Output.Info($"Checking out reference '{reference}'");

        GitRepository.Clone(repository, sourceDirectory.FullName).Checkout(reference);

        return sourceDirectory;
    }

    private static void PrintBenchmarkComparison(BenchmarkComparisonResult results)
    {
        Console.WriteLine();
        PrintHeader("BENCHMARK COMPARISON RESULTS");
        Console.WriteLine();

        if (!results.Comparisons.Any())
        {
            PrintWarning("No benchmark comparisons available.");
            return;
        }

        foreach (var comparison in results.Comparisons)
        {
            PrintBenchmarkHeader(comparison.FullName);
            PrintStatisticsComparison(comparison.Statistics);
            PrintMemoryComparison(comparison.Memory);
            Console.WriteLine();
        }
    }

    private static void PrintHeader(string title)
    {
        Console.ForegroundColor = ConsoleColor.Cyan;
        Console.WriteLine($"=== {title} ===");
        Console.ResetColor();
    }

    private static void PrintBenchmarkHeader(string benchmarkName)
    {
        Console.ForegroundColor = ConsoleColor.Yellow;
        Console.WriteLine($"🔍 {benchmarkName}");
        Console.ResetColor();
    }

    private static void PrintWarning(string message)
    {
        Console.ForegroundColor = ConsoleColor.Yellow;
        Console.WriteLine($"⚠️  {message}");
        Console.ResetColor();
    }

    private static void PrintStatisticsComparison(BenchmarkComparison.StatisticsComparison stats)
    {
        Console.WriteLine("  📊 Performance Statistics:");
        PrintMetric("Mean", stats.Mean, "ns", lowerIsBetter: true);
        PrintMetric("Median", stats.Median, "ns", lowerIsBetter: true);
        PrintMetric("Min", stats.Min, "ns", lowerIsBetter: true);
        PrintMetric("Max", stats.Max, "ns", lowerIsBetter: true);
        PrintMetric("Std Dev", stats.StandardDeviation, "ns", lowerIsBetter: true);

        Console.WriteLine("    📈 Key Percentiles:");
        PrintMetric("P50", stats.Percentiles.P50, "ns", lowerIsBetter: true, indent: 6);
        PrintMetric("P90", stats.Percentiles.P90, "ns", lowerIsBetter: true, indent: 6);
        PrintMetric("P95", stats.Percentiles.P95, "ns", lowerIsBetter: true, indent: 6);
    }

    private static void PrintMemoryComparison(BenchmarkComparison.MemoryMetricsComparison memory)
    {
        Console.WriteLine("  💾 Memory Metrics:");
        PrintMetric(
            "Allocated/Op",
            memory.BytesAllocatedPerOperation,
            "bytes",
            lowerIsBetter: true
        );
        PrintMetric("Gen0 GC", memory.Gen0Collections, "", lowerIsBetter: true);
        PrintMetric("Gen1 GC", memory.Gen1Collections, "", lowerIsBetter: true);
        PrintMetric("Gen2 GC", memory.Gen2Collections, "", lowerIsBetter: true);
    }

    private static void PrintMetric<T>(
        string name,
        ComparisonValue<T> value,
        string unit,
        bool lowerIsBetter,
        int indent = 4
    )
        where T : struct, IComparable<T>, INumber<T>
    {
        var indentStr = new string(' ', indent);
        Console.Write($"{indentStr}{name, -12}: ");

        if (!value.Baseline.HasValue && !value.Target.HasValue)
        {
            Console.ForegroundColor = ConsoleColor.Gray;
            Console.WriteLine("No data available");
            Console.ResetColor();
            return;
        }

        if (!value.Baseline.HasValue)
        {
            Console.ForegroundColor = ConsoleColor.Gray;
            Console.Write("N/A");
            Console.ResetColor();
        }
        else
        {
            Console.Write($"{FormatValue(value.Baseline.Value)}{unit}");
        }

        Console.Write(" → ");

        if (!value.Target.HasValue)
        {
            Console.ForegroundColor = ConsoleColor.Gray;
            Console.Write("N/A");
            Console.ResetColor();
        }
        else
        {
            Console.Write($"{FormatValue(value.Target.Value)}{unit}");
        }

        if (value.Delta.HasValue)
        {
            Console.Write(" (");

            // Set color based on improvement/regression
            Console.ForegroundColor = value.GetChangeColor(lowerIsBetter);
            Console.Write(value.GetChangeSymbol(lowerIsBetter));

            var deltaFormatted = FormatValue(value.Delta.Value);
            var sign = value.Delta.Value.CompareTo(T.Zero) >= 0 ? "+" : "";
            Console.Write($" {sign}{deltaFormatted}{unit}");

            if (
                value.PercentageChange.HasValue
                && value.Baseline.HasValue
                && !value.Baseline.Value.Equals(T.Zero)
            )
            {
                var sign2 = value.PercentageChange.Value >= 0 ? "+" : "";
                Console.Write($", {sign2}{value.PercentageChange.Value:F1}%");
            }

            Console.Write(")");
            Console.ResetColor();
        }

        Console.WriteLine();
    }

    private static string FormatValue<T>(T value)
        where T : struct, INumber<T>
    {
        if (typeof(T) == typeof(double))
        {
            var doubleValue = Convert.ToDouble(value);
            return doubleValue >= 1000 ? $"{doubleValue:F0}" : $"{doubleValue:F2}";
        }
        return value.ToString() ?? "0";
    }
}
