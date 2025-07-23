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
            Comparisons = result
                .Comparisons.Select(c =>
                    CreateComparisonData(c, significanceThreshold: result.SignificanceThreshold)
                )
                .ToArray(),
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
            c.Statistics.Mean.IsImprovement()
            && c.Statistics.Mean.HasSignificantChange(result.SignificanceThreshold * 100)
        );
        var regressions = result.Comparisons.Count(c =>
            c.Statistics.Mean.IsRegression()
            && c.Statistics.Mean.HasSignificantChange(result.SignificanceThreshold * 100)
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

    private static object CreateComparisonData(
        BenchmarkComparison comparison,
        double significanceThreshold
    )
    {
        return new
        {
            comparison.FullName,
            Performance = new
            {
                Mean = CreateValueData(comparison.Statistics.Mean, significanceThreshold),
                Median = CreateValueData(comparison.Statistics.Median, significanceThreshold),
                Min = CreateValueData(comparison.Statistics.Min, significanceThreshold),
                Max = CreateValueData(comparison.Statistics.Max, significanceThreshold),
                StandardDeviation = CreateValueData(
                    comparison.Statistics.StandardDeviation,
                    significanceThreshold
                ),
                StandardError = CreateValueData(
                    comparison.Statistics.StandardError,
                    significanceThreshold
                ),
                Variance = CreateValueData(comparison.Statistics.Variance, significanceThreshold),
                Skewness = CreateValueData(comparison.Statistics.Skewness, significanceThreshold),
                Kurtosis = CreateValueData(comparison.Statistics.Kurtosis, significanceThreshold),
                ConfidenceInterval = new
                {
                    Mean = CreateValueData(
                        comparison.Statistics.ConfidenceInterval.Mean,
                        significanceThreshold
                    ),
                    Lower = CreateValueData(
                        comparison.Statistics.ConfidenceInterval.Lower,
                        significanceThreshold
                    ),
                    Upper = CreateValueData(
                        comparison.Statistics.ConfidenceInterval.Upper,
                        significanceThreshold
                    ),
                    Margin = CreateValueData(
                        comparison.Statistics.ConfidenceInterval.Margin,
                        significanceThreshold
                    ),
                },
                Percentiles = new
                {
                    P0 = CreateValueData(
                        comparison.Statistics.Percentiles.P0,
                        significanceThreshold
                    ),
                    P25 = CreateValueData(
                        comparison.Statistics.Percentiles.P25,
                        significanceThreshold
                    ),
                    P50 = CreateValueData(
                        comparison.Statistics.Percentiles.P50,
                        significanceThreshold
                    ),
                    P67 = CreateValueData(
                        comparison.Statistics.Percentiles.P67,
                        significanceThreshold
                    ),
                    P80 = CreateValueData(
                        comparison.Statistics.Percentiles.P80,
                        significanceThreshold
                    ),
                    P85 = CreateValueData(
                        comparison.Statistics.Percentiles.P85,
                        significanceThreshold
                    ),
                    P90 = CreateValueData(
                        comparison.Statistics.Percentiles.P90,
                        significanceThreshold
                    ),
                    P95 = CreateValueData(
                        comparison.Statistics.Percentiles.P95,
                        significanceThreshold
                    ),
                    P100 = CreateValueData(
                        comparison.Statistics.Percentiles.P100,
                        significanceThreshold
                    ),
                },
            },
            Memory = new
            {
                BytesAllocatedPerOperation = CreateValueData(
                    comparison.Memory.BytesAllocatedPerOperation,
                    significanceThreshold
                ),
                Gen0Collections = CreateValueData(
                    comparison.Memory.Gen0Collections,
                    significanceThreshold
                ),
                Gen1Collections = CreateValueData(
                    comparison.Memory.Gen1Collections,
                    significanceThreshold
                ),
                Gen2Collections = CreateValueData(
                    comparison.Memory.Gen2Collections,
                    significanceThreshold
                ),
                TotalOperations = CreateValueData(
                    comparison.Memory.TotalOperations,
                    significanceThreshold
                ),
            },
        };
    }

    private static object CreateValueData<T>(ComparisonValue<T> value, double significanceThreshold)
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
            HasSignificantChange = value.HasSignificantChange(significanceThreshold * 100),
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
