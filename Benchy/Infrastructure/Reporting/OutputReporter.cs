using System.Globalization;
using Benchy.Core;

namespace Benchy.Infrastructure.Reporting;

public class OutputReporter(TextWriter writer, bool useColors) : IReporter
{
    public void GenerateReport(BenchmarkComparisonResult result)
    {
        writer.WriteLine();
        WriteHeader("Benchmark Comparison Report");
        writer.WriteLine();

        if (result.Comparisons.Count == 0)
        {
            writer.WriteLine("No benchmarks found to compare.");
            return;
        }

        foreach (var comparison in result.Comparisons)
        {
            WriteBenchmarkComparison(comparison);
            writer.WriteLine();
        }

        WriteSummary(result);
    }

    private void WriteBenchmarkComparison(BenchmarkComparison comparison)
    {
        WriteSubHeader(comparison.FullName);

        // Performance metrics
        WriteMetricSection("Performance Metrics");
        WriteMetric("Mean", comparison.Statistics.Mean, "ns", lowerIsBetter: true);
        WriteMetric("Median", comparison.Statistics.Median, "ns", lowerIsBetter: true);
        WriteMetric("Min", comparison.Statistics.Min, "ns", lowerIsBetter: true);
        WriteMetric("Max", comparison.Statistics.Max, "ns", lowerIsBetter: true);
        WriteMetric("Std Dev", comparison.Statistics.StandardDeviation, "ns", lowerIsBetter: true);
        WriteMetric("Std Error", comparison.Statistics.StandardError, "ns", lowerIsBetter: true);

        // Memory metrics
        WriteMetricSection("Memory Metrics");
        WriteMetric(
            "Allocated",
            comparison.Memory.BytesAllocatedPerOperation,
            "B",
            lowerIsBetter: true
        );
        WriteMetric("Gen0", comparison.Memory.Gen0Collections, "", lowerIsBetter: true);
        WriteMetric("Gen1", comparison.Memory.Gen1Collections, "", lowerIsBetter: true);
        WriteMetric("Gen2", comparison.Memory.Gen2Collections, "", lowerIsBetter: true);
    }

    private void WriteMetric<T>(
        string name,
        ComparisonValue<T> value,
        string unit,
        bool lowerIsBetter
    )
        where T : struct, IComparable<T>, System.Numerics.INumber<T>
    {
        var baseline = value.Baseline?.ToString("F2", CultureInfo.InvariantCulture) ?? "N/A";
        var target = value.Target?.ToString("F2", CultureInfo.InvariantCulture) ?? "N/A";
        var delta = value.Delta?.ToString("F2", CultureInfo.InvariantCulture) ?? "N/A";
        var percentage =
            value.PercentageChange?.ToString("F1", CultureInfo.InvariantCulture) ?? "N/A";
        var symbol = value.GetChangeSymbol(lowerIsBetter);

        if (useColors && value.Delta.HasValue)
        {
            var color = value.GetChangeColor(lowerIsBetter);
            var originalColor = Console.ForegroundColor;
            Console.ForegroundColor = color;
            writer.WriteLine(
                $"  {name, -12}: {baseline, 12}{unit} -> {target, 12}{unit} ({delta, 12}{unit}, {percentage, 6}%) {symbol}"
            );
            Console.ForegroundColor = originalColor;
        }
        else
        {
            writer.WriteLine(
                $"  {name, -12}: {baseline, 12}{unit} -> {target, 12}{unit} ({delta, 12}{unit}, {percentage, 6}%) {symbol}"
            );
        }
    }

    private void WriteSummary(BenchmarkComparisonResult result)
    {
        WriteHeader("Summary");

        var totalBenchmarks = result.Comparisons.Count;
        var improvements = result.Comparisons.Count(c =>
            c.Statistics.Mean.IsImprovement(lowerIsBetter: true)
        );
        var regressions = result.Comparisons.Count(c =>
            c.Statistics.Mean.IsRegression(lowerIsBetter: true)
        );
        var unchanged = totalBenchmarks - improvements - regressions;

        writer.WriteLine($"Total Benchmarks: {totalBenchmarks}");
        WriteColoredText($"Improvements: {improvements}", ConsoleColor.Green);
        WriteColoredText($"Regressions: {regressions}", ConsoleColor.Red);
        writer.WriteLine($"Unchanged: {unchanged}");

        if (regressions > 0)
        {
            writer.WriteLine();
            WriteColoredText("⚠️  Performance regressions detected!", ConsoleColor.Yellow);
        }
        else if (improvements > 0)
        {
            writer.WriteLine();
            WriteColoredText("✅ Performance improvements detected!", ConsoleColor.Green);
        }
    }

    private void WriteHeader(string title)
    {
        var border = new string('=', title.Length + 4);
        writer.WriteLine(border);
        writer.WriteLine($"  {title}");
        writer.WriteLine(border);
    }

    private void WriteSubHeader(string title)
    {
        var border = new string('-', Math.Min(title.Length + 4, 80));
        writer.WriteLine(border);
        writer.WriteLine($"  {title}");
        writer.WriteLine(border);
    }

    private void WriteMetricSection(string title)
    {
        writer.WriteLine();
        writer.WriteLine($"{title}:");
    }

    private void WriteColoredText(string text, ConsoleColor color)
    {
        if (useColors)
        {
            var originalColor = Console.ForegroundColor;
            Console.ForegroundColor = color;
            writer.WriteLine(text);
            Console.ForegroundColor = originalColor;
        }
        else
        {
            writer.WriteLine(text);
        }
    }
}
