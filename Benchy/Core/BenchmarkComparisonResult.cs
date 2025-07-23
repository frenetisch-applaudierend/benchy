using System.Numerics;

namespace Benchy.Core;

public sealed record BenchmarkComparisonResult(
    IReadOnlyList<BenchmarkComparison> Comparisons,
    double SignificanceThreshold
)
{
    public static BenchmarkComparisonResult FromBenchmarkRunResults(
        BenchmarkRunResult baseline,
        BenchmarkRunResult target,
        double significanceThreshold
    )
    {
        var baselineBenchmarksByName = baseline
            .Reports.SelectMany(report => report.Benchmarks)
            .ToDictionary(benchmark => benchmark.FullName);

        var targetBenchmarksByName = target
            .Reports.SelectMany(report => report.Benchmarks)
            .ToDictionary(benchmark => benchmark.FullName);

        var allBenchmarkNames = baselineBenchmarksByName
            .Keys.Concat(targetBenchmarksByName.Keys)
            .Distinct()
            .Order();

        var comparisons = allBenchmarkNames
            .Select(name =>
                BenchmarkComparison.FromBenchmarks(
                    baselineBenchmarksByName.GetValueOrDefault(name),
                    targetBenchmarksByName.GetValueOrDefault(name),
                    significanceThreshold
                )
            )
            .ToList();

        return new BenchmarkComparisonResult(comparisons, significanceThreshold);
    }
}

public sealed record BenchmarkComparison(
    string FullName,
    BenchmarkComparison.StatisticsComparison Statistics,
    BenchmarkComparison.MemoryMetricsComparison Memory
)
{
    public static BenchmarkComparison FromBenchmarks(
        BenchmarkReport.Benchmark? baseline,
        BenchmarkReport.Benchmark? target,
        double significanceThreshold
    )
    {
        if (baseline is null && target is null)
        {
            throw new ArgumentException("At least either baseline or target must be provided");
        }

        return new BenchmarkComparison(
            FullName: baseline?.FullName ?? target!.FullName,
            Statistics: StatisticsComparison.FromStatistics(
                baseline?.Statistics,
                target?.Statistics,
                significanceThreshold
            ),
            Memory: MemoryMetricsComparison.FromMemoryMetrics(
                baseline?.Memory,
                target?.Memory,
                significanceThreshold
            )
        );
    }

    public sealed record StatisticsComparison(
        ComparisonValue<double> Mean,
        ComparisonValue<double> Min,
        ComparisonValue<double> Max,
        ComparisonValue<double> Median,
        ComparisonValue<double> StandardDeviation,
        ComparisonValue<double> StandardError,
        ConfidenceIntervalComparison ConfidenceInterval,
        PercentilesComparison Percentiles
    )
    {
        public static StatisticsComparison FromStatistics(
            BenchmarkReport.Statistics? baseline,
            BenchmarkReport.Statistics? target,
            double significanceThreshold
        )
        {
            return new StatisticsComparison(
                Mean: new ComparisonValue<double>(
                    baseline?.Mean,
                    target?.Mean,
                    MetricDirection.LowerIsBetter,
                    significanceThreshold
                ),
                Min: new ComparisonValue<double>(
                    baseline?.Min,
                    target?.Min,
                    MetricDirection.LowerIsBetter,
                    significanceThreshold
                ),
                Max: new ComparisonValue<double>(
                    baseline?.Max,
                    target?.Max,
                    MetricDirection.LowerIsBetter,
                    significanceThreshold
                ),
                Median: new ComparisonValue<double>(
                    baseline?.Median,
                    target?.Median,
                    MetricDirection.LowerIsBetter,
                    significanceThreshold
                ),
                StandardDeviation: new ComparisonValue<double>(
                    baseline?.StandardDeviation,
                    target?.StandardDeviation,
                    MetricDirection.Irrelevant,
                    significanceThreshold
                ),
                StandardError: new ComparisonValue<double>(
                    baseline?.StandardError,
                    target?.StandardError,
                    MetricDirection.Irrelevant,
                    significanceThreshold
                ),
                ConfidenceInterval: ConfidenceIntervalComparison.FromConfidenceInterval(
                    baseline?.ConfidenceInterval,
                    target?.ConfidenceInterval,
                    significanceThreshold
                ),
                Percentiles: PercentilesComparison.FromPercentiles(
                    baseline?.Percentiles,
                    target?.Percentiles,
                    significanceThreshold
                )
            );
        }
    }

    public sealed record MemoryMetricsComparison(
        ComparisonValue<int> BytesAllocatedPerOperation,
        ComparisonValue<int> Gen0Collections,
        ComparisonValue<int> Gen1Collections,
        ComparisonValue<int> Gen2Collections,
        ComparisonValue<long> TotalOperations
    )
    {
        public static MemoryMetricsComparison FromMemoryMetrics(
            BenchmarkReport.MemoryMetrics? baseline,
            BenchmarkReport.MemoryMetrics? target,
            double significanceThreshold
        )
        {
            return new MemoryMetricsComparison(
                BytesAllocatedPerOperation: new ComparisonValue<int>(
                    baseline?.BytesAllocatedPerOperation,
                    target?.BytesAllocatedPerOperation,
                    MetricDirection.LowerIsBetter,
                    significanceThreshold
                ),
                Gen0Collections: new ComparisonValue<int>(
                    baseline?.Gen0Collections,
                    target?.Gen0Collections,
                    MetricDirection.LowerIsBetter,
                    significanceThreshold
                ),
                Gen1Collections: new ComparisonValue<int>(
                    baseline?.Gen1Collections,
                    target?.Gen1Collections,
                    MetricDirection.LowerIsBetter,
                    significanceThreshold
                ),
                Gen2Collections: new ComparisonValue<int>(
                    baseline?.Gen2Collections,
                    target?.Gen2Collections,
                    MetricDirection.LowerIsBetter,
                    significanceThreshold
                ),
                TotalOperations: new ComparisonValue<long>(
                    baseline?.TotalOperations,
                    target?.TotalOperations,
                    MetricDirection.HigherIsBetter,
                    significanceThreshold
                )
            );
        }
    }

    public sealed record ConfidenceIntervalComparison(
        ComparisonValue<double> Mean,
        ComparisonValue<double> Margin,
        ComparisonValue<double> Lower,
        ComparisonValue<double> Upper
    )
    {
        public static ConfidenceIntervalComparison FromConfidenceInterval(
            BenchmarkReport.ConfidenceInterval? baseline,
            BenchmarkReport.ConfidenceInterval? target,
            double significanceThreshold
        )
        {
            return new ConfidenceIntervalComparison(
                Mean: new ComparisonValue<double>(
                    baseline?.Mean,
                    target?.Mean,
                    MetricDirection.LowerIsBetter,
                    significanceThreshold
                ),
                Margin: new ComparisonValue<double>(
                    baseline?.Margin,
                    target?.Margin,
                    MetricDirection.LowerIsBetter,
                    significanceThreshold
                ),
                Lower: new ComparisonValue<double>(
                    baseline?.Lower,
                    target?.Lower,
                    MetricDirection.LowerIsBetter,
                    significanceThreshold
                ),
                Upper: new ComparisonValue<double>(
                    baseline?.Upper,
                    target?.Upper,
                    MetricDirection.LowerIsBetter,
                    significanceThreshold
                )
            );
        }
    }

    public sealed record PercentilesComparison(
        ComparisonValue<double> P0,
        ComparisonValue<double> P25,
        ComparisonValue<double> P50,
        ComparisonValue<double> P67,
        ComparisonValue<double> P80,
        ComparisonValue<double> P85,
        ComparisonValue<double> P90,
        ComparisonValue<double> P95,
        ComparisonValue<double> P100
    )
    {
        public static PercentilesComparison FromPercentiles(
            BenchmarkReport.Percentiles? baseline,
            BenchmarkReport.Percentiles? target,
            double significanceThreshold
        )
        {
            return new PercentilesComparison(
                P0: new ComparisonValue<double>(
                    baseline?.P0,
                    target?.P0,
                    MetricDirection.LowerIsBetter,
                    significanceThreshold
                ),
                P25: new ComparisonValue<double>(
                    baseline?.P25,
                    target?.P25,
                    MetricDirection.LowerIsBetter,
                    significanceThreshold
                ),
                P50: new ComparisonValue<double>(
                    baseline?.P50,
                    target?.P50,
                    MetricDirection.LowerIsBetter,
                    significanceThreshold
                ),
                P67: new ComparisonValue<double>(
                    baseline?.P67,
                    target?.P67,
                    MetricDirection.LowerIsBetter,
                    significanceThreshold
                ),
                P80: new ComparisonValue<double>(
                    baseline?.P80,
                    target?.P80,
                    MetricDirection.LowerIsBetter,
                    significanceThreshold
                ),
                P85: new ComparisonValue<double>(
                    baseline?.P85,
                    target?.P85,
                    MetricDirection.LowerIsBetter,
                    significanceThreshold
                ),
                P90: new ComparisonValue<double>(
                    baseline?.P90,
                    target?.P90,
                    MetricDirection.LowerIsBetter,
                    significanceThreshold
                ),
                P95: new ComparisonValue<double>(
                    baseline?.P95,
                    target?.P95,
                    MetricDirection.LowerIsBetter,
                    significanceThreshold
                ),
                P100: new ComparisonValue<double>(
                    baseline?.P100,
                    target?.P100,
                    MetricDirection.LowerIsBetter,
                    significanceThreshold
                )
            );
        }
    }
}

public sealed record ComparisonValue<T>(
    T? Baseline,
    T? Target,
    MetricDirection Direction,
    double SignificanceThreshold
)
    where T : struct, IComparable<T>, INumber<T>
{
    public T? Delta => Baseline.HasValue && Target.HasValue ? Target.Value - Baseline.Value : null;

    public double? PercentageChange
    {
        get
        {
            if (!Baseline.HasValue || !Target.HasValue || Baseline.Value.Equals(T.Zero))
                return null;

            var baselineDouble = Convert.ToDouble(Baseline.Value);
            var deltaDouble = Convert.ToDouble(Delta!.Value);
            return deltaDouble / baselineDouble * 100.0;
        }
    }

    public bool IsImprovement() =>
        Delta.HasValue
        && Direction switch
        {
            MetricDirection.LowerIsBetter => Delta.Value.CompareTo(T.Zero) < 0,
            MetricDirection.HigherIsBetter => Delta.Value.CompareTo(T.Zero) > 0,
            MetricDirection.Irrelevant => false,
            _ => false,
        };

    public bool IsRegression() =>
        Delta.HasValue
        && Direction switch
        {
            MetricDirection.LowerIsBetter => Delta.Value.CompareTo(T.Zero) > 0,
            MetricDirection.HigherIsBetter => Delta.Value.CompareTo(T.Zero) < 0,
            MetricDirection.Irrelevant => false,
            _ => false,
        };

    public bool HasSignificantChange() =>
        PercentageChange.HasValue
        && Math.Abs(PercentageChange.Value) >= SignificanceThreshold * 100;

    public bool IsSignificantImprovement() => IsImprovement() && HasSignificantChange();

    public bool IsSignificantRegression() => IsRegression() && HasSignificantChange();
}

public enum MetricDirection
{
    LowerIsBetter,
    HigherIsBetter,
    Irrelevant,
}
