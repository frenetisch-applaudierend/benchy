using System.Text.Json;
using System.Text.Json.Serialization;
using Benchy.Core;

namespace Benchy.Infrastructure.Reporting;

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
            Comparisons = result.Comparisons.Select(CreateComparisonData).ToArray(),
        };

        var json = JsonSerializer.Serialize(reportData, jsonOptions);

        var directory = Path.GetDirectoryName(outputPath);
        if (!string.IsNullOrEmpty(directory))
        {
            Directory.CreateDirectory(directory);
        }

        File.WriteAllText(outputPath, json);
    }

    private static object CreateSummary(BenchmarkComparisonResult result)
    {
        var improvements = result.Comparisons.Count(c =>
            c.Statistics.Mean.IsImprovement(lowerIsBetter: true)
        );
        var regressions = result.Comparisons.Count(c =>
            c.Statistics.Mean.IsRegression(lowerIsBetter: true)
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
                Mean = CreateValueData(comparison.Statistics.Mean, lowerIsBetter: true),
                Median = CreateValueData(comparison.Statistics.Median, lowerIsBetter: true),
                Min = CreateValueData(comparison.Statistics.Min, lowerIsBetter: true),
                Max = CreateValueData(comparison.Statistics.Max, lowerIsBetter: true),
                StandardDeviation = CreateValueData(
                    comparison.Statistics.StandardDeviation,
                    lowerIsBetter: true
                ),
                StandardError = CreateValueData(
                    comparison.Statistics.StandardError,
                    lowerIsBetter: true
                ),
                Variance = CreateValueData(comparison.Statistics.Variance, lowerIsBetter: true),
                Skewness = CreateValueData(comparison.Statistics.Skewness, lowerIsBetter: true),
                Kurtosis = CreateValueData(comparison.Statistics.Kurtosis, lowerIsBetter: true),
                ConfidenceInterval = new
                {
                    Mean = CreateValueData(
                        comparison.Statistics.ConfidenceInterval.Mean,
                        lowerIsBetter: true
                    ),
                    Lower = CreateValueData(
                        comparison.Statistics.ConfidenceInterval.Lower,
                        lowerIsBetter: true
                    ),
                    Upper = CreateValueData(
                        comparison.Statistics.ConfidenceInterval.Upper,
                        lowerIsBetter: true
                    ),
                    Margin = CreateValueData(
                        comparison.Statistics.ConfidenceInterval.Margin,
                        lowerIsBetter: true
                    ),
                },
                Percentiles = new
                {
                    P0 = CreateValueData(comparison.Statistics.Percentiles.P0, lowerIsBetter: true),
                    P25 = CreateValueData(
                        comparison.Statistics.Percentiles.P25,
                        lowerIsBetter: true
                    ),
                    P50 = CreateValueData(
                        comparison.Statistics.Percentiles.P50,
                        lowerIsBetter: true
                    ),
                    P67 = CreateValueData(
                        comparison.Statistics.Percentiles.P67,
                        lowerIsBetter: true
                    ),
                    P80 = CreateValueData(
                        comparison.Statistics.Percentiles.P80,
                        lowerIsBetter: true
                    ),
                    P85 = CreateValueData(
                        comparison.Statistics.Percentiles.P85,
                        lowerIsBetter: true
                    ),
                    P90 = CreateValueData(
                        comparison.Statistics.Percentiles.P90,
                        lowerIsBetter: true
                    ),
                    P95 = CreateValueData(
                        comparison.Statistics.Percentiles.P95,
                        lowerIsBetter: true
                    ),
                    P100 = CreateValueData(
                        comparison.Statistics.Percentiles.P100,
                        lowerIsBetter: true
                    ),
                },
            },
            Memory = new
            {
                BytesAllocatedPerOperation = CreateValueData(
                    comparison.Memory.BytesAllocatedPerOperation,
                    lowerIsBetter: true
                ),
                Gen0Collections = CreateValueData(
                    comparison.Memory.Gen0Collections,
                    lowerIsBetter: true
                ),
                Gen1Collections = CreateValueData(
                    comparison.Memory.Gen1Collections,
                    lowerIsBetter: true
                ),
                Gen2Collections = CreateValueData(
                    comparison.Memory.Gen2Collections,
                    lowerIsBetter: true
                ),
                TotalOperations = CreateValueData(
                    comparison.Memory.TotalOperations,
                    lowerIsBetter: false
                ),
            },
        };
    }

    private static object CreateValueData<T>(ComparisonValue<T> value, bool lowerIsBetter = true)
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
            HasSignificantChange = value.HasSignificantChange(),
            ChangeSymbol = value.GetChangeSymbol(lowerIsBetter),
        };
    }
}
