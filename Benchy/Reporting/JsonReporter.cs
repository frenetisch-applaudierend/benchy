using System.Text.Json;
using System.Text.Json.Serialization;
using Benchy.Core;
using Benchy.Output;
using static Benchy.Output.FormattedText;

namespace Benchy.Reporting;

public class JsonReporter : IReporter
{
    private readonly string outputPath;
    private readonly JsonSerializerOptions jsonOptions;

    public JsonReporter(string outputPath)
    {
        this.outputPath = outputPath;
        jsonOptions = new JsonSerializerOptions
        {
            WriteIndented = true,
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
        };
    }

    public void GenerateReport(BenchmarkComparisonResult result)
    {
        var reportData = new
        {
            GeneratedAt = DateTime.UtcNow,
            TotalBenchmarks = result.Comparisons.Count,
            Summary = CreateSummary(result),
            Comparisons = result.Comparisons.Select(c => CreateComparisonData(c)).ToArray(),
        };

        var json = JsonSerializer.Serialize(reportData, jsonOptions);

        var directory = Path.GetDirectoryName(outputPath);
        if (!string.IsNullOrEmpty(directory))
        {
            Directory.CreateDirectory(directory);
        }

        File.WriteAllText(outputPath, json);

        CliOutput.Info($"JSON report saved to {Em(outputPath)}");
    }

    private static object CreateSummary(BenchmarkComparisonResult result)
    {
        var improvements = result.Comparisons.Count(c =>
            c.Statistics.Mean.IsSignificantImprovement()
        );
        var regressions = result.Comparisons.Count(c =>
            c.Statistics.Mean.IsSignificantRegression()
        );
        var unchanged = result.Comparisons.Count - improvements - regressions;

        return new
        {
            TotalBenchmarks = result.Comparisons.Count,
            Improvements = improvements,
            Regressions = regressions,
            Unchanged = unchanged,
            HasRegressions = regressions > 0,
            HasImprovements = improvements > 0,
        };
    }

    private static object CreateComparisonData(BenchmarkComparison comparison)
    {
        return new
        {
            comparison.FullName,
            Performance = new
            {
                Mean = CreateValueData(comparison.Statistics.Mean),
                Median = CreateValueData(comparison.Statistics.Median),
                Min = CreateValueData(comparison.Statistics.Min),
                Max = CreateValueData(comparison.Statistics.Max),
                StandardDeviation = CreateValueData(comparison.Statistics.StandardDeviation),
                StandardError = CreateValueData(comparison.Statistics.StandardError),
                Variance = CreateValueData(comparison.Statistics.Variance),
                Skewness = CreateValueData(comparison.Statistics.Skewness),
                Kurtosis = CreateValueData(comparison.Statistics.Kurtosis),
                ConfidenceInterval = new
                {
                    Mean = CreateValueData(comparison.Statistics.ConfidenceInterval.Mean),
                    Lower = CreateValueData(comparison.Statistics.ConfidenceInterval.Lower),
                    Upper = CreateValueData(comparison.Statistics.ConfidenceInterval.Upper),
                    Margin = CreateValueData(comparison.Statistics.ConfidenceInterval.Margin),
                },
                Percentiles = new
                {
                    P0 = CreateValueData(comparison.Statistics.Percentiles.P0),
                    P25 = CreateValueData(comparison.Statistics.Percentiles.P25),
                    P50 = CreateValueData(comparison.Statistics.Percentiles.P50),
                    P67 = CreateValueData(comparison.Statistics.Percentiles.P67),
                    P80 = CreateValueData(comparison.Statistics.Percentiles.P80),
                    P85 = CreateValueData(comparison.Statistics.Percentiles.P85),
                    P90 = CreateValueData(comparison.Statistics.Percentiles.P90),
                    P95 = CreateValueData(comparison.Statistics.Percentiles.P95),
                    P100 = CreateValueData(comparison.Statistics.Percentiles.P100),
                },
            },
            Memory = new
            {
                BytesAllocatedPerOperation = CreateValueData(
                    comparison.Memory.BytesAllocatedPerOperation
                ),
                Gen0Collections = CreateValueData(comparison.Memory.Gen0Collections),
                Gen1Collections = CreateValueData(comparison.Memory.Gen1Collections),
                Gen2Collections = CreateValueData(comparison.Memory.Gen2Collections),
                TotalOperations = CreateValueData(comparison.Memory.TotalOperations),
            },
        };
    }

    private static object CreateValueData<T>(ComparisonValue<T> value)
        where T : struct, IComparable<T>, System.Numerics.INumber<T>
    {
        return new
        {
            Baseline = value.Baseline,
            Target = value.Target,
            Delta = value.Delta,
            PercentageChange = value.PercentageChange,
            IsImprovement = value.IsImprovement(),
            IsRegression = value.IsRegression(),
            HasSignificantChange = value.HasSignificantChange(),
            IsSignificantImprovement = value.IsSignificantImprovement(),
            IsSignificantRegression = value.IsSignificantRegression(),
            ChangeSymbol = GetChangeSymbol(value),
        };
    }

    private static string GetChangeSymbol<T>(ComparisonValue<T> value)
        where T : struct, IComparable<T>, System.Numerics.INumber<T>
    {
        if (!value.Delta.HasValue)
            return "?";
        if (value.Delta.Value.Equals(T.Zero))
            return "=";
        return value.IsImprovement() ? "✓" : "✗";
    }
}
