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
            c.Statistics.Mean.IsImprovement(lowerIsBetter: true)
            && c.Statistics.Mean.HasSignificantChange(result.SignificanceThreshold * 100)
        );
        var regressions = result.Comparisons.Count(c =>
            c.Statistics.Mean.IsRegression(lowerIsBetter: true)
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
                Mean = CreateValueData(
                    comparison.Statistics.Mean,
                    lowerIsBetter: true,
                    significanceThreshold
                ),
                Median = CreateValueData(
                    comparison.Statistics.Median,
                    lowerIsBetter: true,
                    significanceThreshold
                ),
                Min = CreateValueData(
                    comparison.Statistics.Min,
                    lowerIsBetter: true,
                    significanceThreshold
                ),
                Max = CreateValueData(
                    comparison.Statistics.Max,
                    lowerIsBetter: true,
                    significanceThreshold
                ),
                StandardDeviation = CreateValueData(
                    comparison.Statistics.StandardDeviation,
                    lowerIsBetter: true,
                    significanceThreshold
                ),
                StandardError = CreateValueData(
                    comparison.Statistics.StandardError,
                    lowerIsBetter: true,
                    significanceThreshold
                ),
                Variance = CreateValueData(
                    comparison.Statistics.Variance,
                    lowerIsBetter: true,
                    significanceThreshold
                ),
                Skewness = CreateValueData(
                    comparison.Statistics.Skewness,
                    lowerIsBetter: true,
                    significanceThreshold
                ),
                Kurtosis = CreateValueData(
                    comparison.Statistics.Kurtosis,
                    lowerIsBetter: true,
                    significanceThreshold
                ),
                ConfidenceInterval = new
                {
                    Mean = CreateValueData(
                        comparison.Statistics.ConfidenceInterval.Mean,
                        lowerIsBetter: true,
                        significanceThreshold
                    ),
                    Lower = CreateValueData(
                        comparison.Statistics.ConfidenceInterval.Lower,
                        lowerIsBetter: true,
                        significanceThreshold
                    ),
                    Upper = CreateValueData(
                        comparison.Statistics.ConfidenceInterval.Upper,
                        lowerIsBetter: true,
                        significanceThreshold
                    ),
                    Margin = CreateValueData(
                        comparison.Statistics.ConfidenceInterval.Margin,
                        lowerIsBetter: true,
                        significanceThreshold
                    ),
                },
                Percentiles = new
                {
                    P0 = CreateValueData(
                        comparison.Statistics.Percentiles.P0,
                        lowerIsBetter: true,
                        significanceThreshold
                    ),
                    P25 = CreateValueData(
                        comparison.Statistics.Percentiles.P25,
                        lowerIsBetter: true,
                        significanceThreshold
                    ),
                    P50 = CreateValueData(
                        comparison.Statistics.Percentiles.P50,
                        lowerIsBetter: true,
                        significanceThreshold
                    ),
                    P67 = CreateValueData(
                        comparison.Statistics.Percentiles.P67,
                        lowerIsBetter: true,
                        significanceThreshold
                    ),
                    P80 = CreateValueData(
                        comparison.Statistics.Percentiles.P80,
                        lowerIsBetter: true,
                        significanceThreshold
                    ),
                    P85 = CreateValueData(
                        comparison.Statistics.Percentiles.P85,
                        lowerIsBetter: true,
                        significanceThreshold
                    ),
                    P90 = CreateValueData(
                        comparison.Statistics.Percentiles.P90,
                        lowerIsBetter: true,
                        significanceThreshold
                    ),
                    P95 = CreateValueData(
                        comparison.Statistics.Percentiles.P95,
                        lowerIsBetter: true,
                        significanceThreshold
                    ),
                    P100 = CreateValueData(
                        comparison.Statistics.Percentiles.P100,
                        lowerIsBetter: true,
                        significanceThreshold
                    ),
                },
            },
            Memory = new
            {
                BytesAllocatedPerOperation = CreateValueData(
                    comparison.Memory.BytesAllocatedPerOperation,
                    lowerIsBetter: true,
                    significanceThreshold
                ),
                Gen0Collections = CreateValueData(
                    comparison.Memory.Gen0Collections,
                    lowerIsBetter: true,
                    significanceThreshold
                ),
                Gen1Collections = CreateValueData(
                    comparison.Memory.Gen1Collections,
                    lowerIsBetter: true,
                    significanceThreshold
                ),
                Gen2Collections = CreateValueData(
                    comparison.Memory.Gen2Collections,
                    lowerIsBetter: true,
                    significanceThreshold
                ),
                TotalOperations = CreateValueData(
                    comparison.Memory.TotalOperations,
                    lowerIsBetter: false,
                    significanceThreshold
                ),
            },
        };
    }

    private static object CreateValueData<T>(
        ComparisonValue<T> value,
        bool lowerIsBetter,
        double significanceThreshold
    )
        where T : struct, IComparable<T>, System.Numerics.INumber<T>
    {
        return new
        {
            Baseline = value.Baseline,
            Target = value.Target,
            Delta = value.Delta,
            PercentageChange = value.PercentageChange,
            IsImprovement = value.IsImprovement(lowerIsBetter),
            IsRegression = value.IsRegression(lowerIsBetter),
            HasSignificantChange = value.HasSignificantChange(significanceThreshold * 100),
            ChangeSymbol = GetChangeSymbol(value, lowerIsBetter),
        };
    }

    private static string GetChangeSymbol<T>(ComparisonValue<T> value, bool lowerIsBetter = true)
        where T : struct, IComparable<T>, System.Numerics.INumber<T>
    {
        if (!value.Delta.HasValue)
            return "?";
        if (value.Delta.Value.Equals(T.Zero))
            return "=";
        return value.IsImprovement(lowerIsBetter) ? "✓" : "✗";
    }
}
