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
                    targetBenchmarksByName.GetValueOrDefault(name)
                )
            )
            .ToList();

        return new BenchmarkComparisonResult(comparisons, significanceThreshold);
    }

    public bool IsSignificantImprovement(BenchmarkComparison comparison) =>
        comparison.Statistics.Mean.IsImprovement()
        && comparison.Statistics.Mean.HasSignificantChange(SignificanceThreshold * 100);

    public bool IsSignificantRegression(BenchmarkComparison comparison) =>
        comparison.Statistics.Mean.IsRegression()
        && comparison.Statistics.Mean.HasSignificantChange(SignificanceThreshold * 100);
}

public sealed record BenchmarkComparison(
    string FullName,
    BenchmarkComparison.StatisticsComparison Statistics,
    BenchmarkComparison.MemoryMetricsComparison Memory
)
{
    public static BenchmarkComparison FromBenchmarks(
        BenchmarkReport.Benchmark? baseline,
        BenchmarkReport.Benchmark? target
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
                target?.Statistics
            ),
            Memory: MemoryMetricsComparison.FromMemoryMetrics(baseline?.Memory, target?.Memory)
        );
    }

    public sealed record StatisticsComparison(
        ComparisonValue<double> Mean,
        ComparisonValue<double> Min,
        ComparisonValue<double> Max,
        ComparisonValue<double> Median,
        ComparisonValue<double> StandardDeviation,
        ComparisonValue<double> StandardError,
        ComparisonValue<double> Variance,
        ComparisonValue<double> Skewness,
        ComparisonValue<double> Kurtosis,
        ConfidenceIntervalComparison ConfidenceInterval,
        PercentilesComparison Percentiles
    )
    {
        public static StatisticsComparison FromStatistics(
            BenchmarkReport.Statistics? baseline,
            BenchmarkReport.Statistics? target
        )
        {
            return new StatisticsComparison(
                Mean: new ComparisonValue<double>(
                    baseline?.Mean,
                    target?.Mean,
                    MetricDirection.LowerIsBetter
                ),
                Min: new ComparisonValue<double>(
                    baseline?.Min,
                    target?.Min,
                    MetricDirection.LowerIsBetter
                ),
                Max: new ComparisonValue<double>(
                    baseline?.Max,
                    target?.Max,
                    MetricDirection.LowerIsBetter
                ),
                Median: new ComparisonValue<double>(
                    baseline?.Median,
                    target?.Median,
                    MetricDirection.LowerIsBetter
                ),
                StandardDeviation: new ComparisonValue<double>(
                    baseline?.StandardDeviation,
                    target?.StandardDeviation,
                    MetricDirection.Irrelevant
                ),
                StandardError: new ComparisonValue<double>(
                    baseline?.StandardError,
                    target?.StandardError,
                    MetricDirection.Irrelevant
                ),
                Variance: new ComparisonValue<double>(
                    baseline?.Variance,
                    target?.Variance,
                    MetricDirection.Irrelevant
                ),
                Skewness: new ComparisonValue<double>(
                    baseline?.Skewness,
                    target?.Skewness,
                    MetricDirection.Irrelevant
                ),
                Kurtosis: new ComparisonValue<double>(
                    baseline?.Kurtosis,
                    target?.Kurtosis,
                    MetricDirection.Irrelevant
                ),
                ConfidenceInterval: ConfidenceIntervalComparison.FromConfidenceInterval(
                    baseline?.ConfidenceInterval,
                    target?.ConfidenceInterval
                ),
                Percentiles: PercentilesComparison.FromPercentiles(
                    baseline?.Percentiles,
                    target?.Percentiles
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
            BenchmarkReport.MemoryMetrics? target
        )
        {
            return new MemoryMetricsComparison(
                BytesAllocatedPerOperation: new ComparisonValue<int>(
                    baseline?.BytesAllocatedPerOperation,
                    target?.BytesAllocatedPerOperation,
                    MetricDirection.LowerIsBetter
                ),
                Gen0Collections: new ComparisonValue<int>(
                    baseline?.Gen0Collections,
                    target?.Gen0Collections,
                    MetricDirection.LowerIsBetter
                ),
                Gen1Collections: new ComparisonValue<int>(
                    baseline?.Gen1Collections,
                    target?.Gen1Collections,
                    MetricDirection.LowerIsBetter
                ),
                Gen2Collections: new ComparisonValue<int>(
                    baseline?.Gen2Collections,
                    target?.Gen2Collections,
                    MetricDirection.LowerIsBetter
                ),
                TotalOperations: new ComparisonValue<long>(
                    baseline?.TotalOperations,
                    target?.TotalOperations,
                    MetricDirection.HigherIsBetter
                )
            );
        }
    }

    public sealed record ConfidenceIntervalComparison(
        ComparisonValue<int> N,
        ComparisonValue<double> Mean,
        ComparisonValue<double> StandardError,
        ComparisonValue<int> Level,
        ComparisonValue<double> Margin,
        ComparisonValue<double> Lower,
        ComparisonValue<double> Upper
    )
    {
        public static ConfidenceIntervalComparison FromConfidenceInterval(
            BenchmarkReport.ConfidenceInterval? baseline,
            BenchmarkReport.ConfidenceInterval? target
        )
        {
            return new ConfidenceIntervalComparison(
                N: new ComparisonValue<int>(baseline?.N, target?.N, MetricDirection.Irrelevant),
                Mean: new ComparisonValue<double>(
                    baseline?.Mean,
                    target?.Mean,
                    MetricDirection.LowerIsBetter
                ),
                StandardError: new ComparisonValue<double>(
                    baseline?.StandardError,
                    target?.StandardError,
                    MetricDirection.Irrelevant
                ),
                Level: new ComparisonValue<int>(
                    baseline?.Level,
                    target?.Level,
                    MetricDirection.Irrelevant
                ),
                Margin: new ComparisonValue<double>(
                    baseline?.Margin,
                    target?.Margin,
                    MetricDirection.LowerIsBetter
                ),
                Lower: new ComparisonValue<double>(
                    baseline?.Lower,
                    target?.Lower,
                    MetricDirection.LowerIsBetter
                ),
                Upper: new ComparisonValue<double>(
                    baseline?.Upper,
                    target?.Upper,
                    MetricDirection.LowerIsBetter
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
            BenchmarkReport.Percentiles? target
        )
        {
            return new PercentilesComparison(
                P0: new ComparisonValue<double>(
                    baseline?.P0,
                    target?.P0,
                    MetricDirection.LowerIsBetter
                ),
                P25: new ComparisonValue<double>(
                    baseline?.P25,
                    target?.P25,
                    MetricDirection.LowerIsBetter
                ),
                P50: new ComparisonValue<double>(
                    baseline?.P50,
                    target?.P50,
                    MetricDirection.LowerIsBetter
                ),
                P67: new ComparisonValue<double>(
                    baseline?.P67,
                    target?.P67,
                    MetricDirection.LowerIsBetter
                ),
                P80: new ComparisonValue<double>(
                    baseline?.P80,
                    target?.P80,
                    MetricDirection.LowerIsBetter
                ),
                P85: new ComparisonValue<double>(
                    baseline?.P85,
                    target?.P85,
                    MetricDirection.LowerIsBetter
                ),
                P90: new ComparisonValue<double>(
                    baseline?.P90,
                    target?.P90,
                    MetricDirection.LowerIsBetter
                ),
                P95: new ComparisonValue<double>(
                    baseline?.P95,
                    target?.P95,
                    MetricDirection.LowerIsBetter
                ),
                P100: new ComparisonValue<double>(
                    baseline?.P100,
                    target?.P100,
                    MetricDirection.LowerIsBetter
                )
            );
        }
    }
}

public sealed record ComparisonValue<T>(T? Baseline, T? Target, MetricDirection Direction)
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

    public bool HasSignificantChange(double thresholdPercent) =>
        PercentageChange.HasValue && Math.Abs(PercentageChange.Value) >= thresholdPercent;
}

public enum MetricDirection
{
    LowerIsBetter,
    HigherIsBetter,
    Irrelevant,
}
