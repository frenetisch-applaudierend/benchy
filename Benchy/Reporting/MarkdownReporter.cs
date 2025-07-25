using System.Globalization;
using System.Text;
using Benchy.Core;
using Benchy.Output;
using static Benchy.Output.FormattedText;

namespace Benchy.Reporting;

public class MarkdownReporter(string outputPath) : IReporter
{
    public void GenerateReport(BenchmarkComparisonResult result)
    {
        var markdown = new StringBuilder();

        WriteHeader(markdown, "Benchmark Comparison Report", 1);
        markdown.AppendLine();

        if (result.Comparisons.Count == 0)
        {
            markdown.AppendLine("No benchmarks found to compare.");
            SaveToFile(markdown.ToString());
            return;
        }

        WriteSummary(markdown, result);
        markdown.AppendLine();

        WriteDetailedResults(markdown, result);

        SaveToFile(markdown.ToString());
    }

    private void WriteSummary(StringBuilder markdown, BenchmarkComparisonResult result)
    {
        WriteHeader(markdown, "Summary", 2);
        markdown.AppendLine();

        var totalBenchmarks = result.Comparisons.Count;
        var improvements = result.Comparisons.Count(c =>
            c.Statistics.Mean.IsSignificantImprovement()
        );
        var regressions = result.Comparisons.Count(c =>
            c.Statistics.Mean.IsSignificantRegression()
        );
        var unchanged = totalBenchmarks - improvements - regressions;

        markdown.AppendLine($"- **Total Benchmarks**: {totalBenchmarks}");
        markdown.AppendLine($"- **Improvements**: {improvements} :green_circle:");
        markdown.AppendLine($"- **Regressions**: {regressions} :red_circle:");
        markdown.AppendLine($"- **Unchanged**: {unchanged} :white_circle:");
        markdown.AppendLine();

        if (regressions > 0)
        {
            markdown.AppendLine("> :warning: **Performance regressions detected!**");
        }
        else if (improvements > 0)
        {
            markdown.AppendLine("> :white_check_mark: **Performance improvements detected!**");
        }
        else
        {
            markdown.AppendLine(
                "> :information_source: **No significant performance changes detected.**"
            );
        }
    }

    private void WriteDetailedResults(StringBuilder markdown, BenchmarkComparisonResult result)
    {
        WriteHeader(markdown, "Detailed Results", 2);
        markdown.AppendLine();

        foreach (var comparison in result.Comparisons)
        {
            WriteBenchmarkComparison(markdown, comparison);
            markdown.AppendLine();
        }
    }

    private void WriteBenchmarkComparison(StringBuilder markdown, BenchmarkComparison comparison)
    {
        WriteHeader(markdown, comparison.FullName, 3);
        markdown.AppendLine();

        // Performance metrics table
        WriteHeader(markdown, "Performance Metrics", 4);
        markdown.AppendLine();

        markdown.AppendLine("| Metric | Baseline | Target | Delta | % Change | Status |");
        markdown.AppendLine("|--------|----------|--------|-------|----------|--------|");

        WriteMetricRow(markdown, "Mean", comparison.Statistics.Mean, "ns");
        WriteMetricRow(markdown, "Median", comparison.Statistics.Median, "ns");
        WriteMetricRow(markdown, "Min", comparison.Statistics.Min, "ns");
        WriteMetricRow(markdown, "Max", comparison.Statistics.Max, "ns");
        WriteMetricRow(markdown, "Std Dev", comparison.Statistics.StandardDeviation, "ns");
        WriteMetricRow(markdown, "Std Error", comparison.Statistics.StandardError, "ns");

        markdown.AppendLine();

        // Memory metrics table
        WriteHeader(markdown, "Memory Metrics", 4);
        markdown.AppendLine();

        markdown.AppendLine("| Metric | Baseline | Target | Delta | % Change | Status |");
        markdown.AppendLine("|--------|----------|--------|-------|----------|--------|");

        WriteMetricRow(markdown, "Allocated", comparison.Memory.BytesAllocatedPerOperation, "B");
        WriteMetricRow(markdown, "Gen0", comparison.Memory.Gen0Collections, "");
        WriteMetricRow(markdown, "Gen1", comparison.Memory.Gen1Collections, "");
        WriteMetricRow(markdown, "Gen2", comparison.Memory.Gen2Collections, "");

        markdown.AppendLine();

        // Percentiles table
        WriteHeader(markdown, "Percentiles", 4);
        markdown.AppendLine();

        markdown.AppendLine("| Percentile | Baseline | Target | Delta | % Change | Status |");
        markdown.AppendLine("|------------|----------|--------|-------|----------|--------|");

        WriteMetricRow(markdown, "P0", comparison.Statistics.Percentiles.P0, "ns");
        WriteMetricRow(markdown, "P25", comparison.Statistics.Percentiles.P25, "ns");
        WriteMetricRow(markdown, "P50", comparison.Statistics.Percentiles.P50, "ns");
        WriteMetricRow(markdown, "P67", comparison.Statistics.Percentiles.P67, "ns");
        WriteMetricRow(markdown, "P80", comparison.Statistics.Percentiles.P80, "ns");
        WriteMetricRow(markdown, "P85", comparison.Statistics.Percentiles.P85, "ns");
        WriteMetricRow(markdown, "P90", comparison.Statistics.Percentiles.P90, "ns");
        WriteMetricRow(markdown, "P95", comparison.Statistics.Percentiles.P95, "ns");
        WriteMetricRow(markdown, "P100", comparison.Statistics.Percentiles.P100, "ns");
    }

    private void WriteMetricRow<T>(
        StringBuilder markdown,
        string name,
        ComparisonValue<T> value,
        string unit
    )
        where T : struct, IComparable<T>, System.Numerics.INumber<T>
    {
        var baseline = value.Baseline?.ToString("F2", CultureInfo.InvariantCulture) ?? "N/A";
        var target = value.Target?.ToString("F2", CultureInfo.InvariantCulture) ?? "N/A";
        var delta = value.Delta?.ToString("F2", CultureInfo.InvariantCulture) ?? "N/A";
        var percentage =
            value.PercentageChange?.ToString("F1", CultureInfo.InvariantCulture) ?? "N/A";
        var status = GetStatusEmoji(value);

        var baselineText = baseline == "N/A" ? baseline : $"{baseline} {unit}";
        var targetText = target == "N/A" ? target : $"{target} {unit}";
        var deltaText = delta == "N/A" ? delta : $"{delta} {unit}";
        var percentageText = percentage == "N/A" ? percentage : $"{percentage}%";

        markdown.AppendLine(
            $"| {name} | {baselineText} | {targetText} | {deltaText} | {percentageText} | {status} |"
        );
    }

    private static string GetStatusEmoji<T>(ComparisonValue<T> value)
        where T : struct, IComparable<T>, System.Numerics.INumber<T>
    {
        if (!value.Delta.HasValue)
            return ":question:";

        if (value.Delta.Value.Equals(default(T)))
            return ":white_circle:";

        // Only show improvement/regression if the change is significant
        if (!value.HasSignificantChange())
            return ":white_circle:";

        if (value.IsImprovement())
            return ":green_circle:";

        if (value.IsRegression())
            return ":red_circle:";

        return ":white_circle:";
    }

    private static void WriteHeader(StringBuilder markdown, string title, int level)
    {
        var prefix = new string('#', level);
        markdown.AppendLine($"{prefix} {title}");
    }

    private void SaveToFile(string content)
    {
        var directory = Path.GetDirectoryName(outputPath);
        if (!string.IsNullOrEmpty(directory))
        {
            Directory.CreateDirectory(directory);
        }

        File.WriteAllText(outputPath, content);

        CliOutput.Info($"Markdown report saved to {Em(outputPath)}");
    }
}
