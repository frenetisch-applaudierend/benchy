using System.Numerics;

namespace Benchy.Core;

public sealed record BenchmarkComparisonResult(IReadOnlyList<BenchmarkComparison> Comparisons)
{
    public static BenchmarkComparisonResult FromBenchmarkRunResults(
        BenchmarkRunResult baseline,
        BenchmarkRunResult target
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

        return new BenchmarkComparisonResult(comparisons);
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
                Mean: new ComparisonValue<double>(baseline?.Mean, target?.Mean),
                Min: new ComparisonValue<double>(baseline?.Min, target?.Min),
                Max: new ComparisonValue<double>(baseline?.Max, target?.Max),
                Median: new ComparisonValue<double>(baseline?.Median, target?.Median),
                StandardDeviation: new ComparisonValue<double>(
                    baseline?.StandardDeviation,
                    target?.StandardDeviation
                ),
                StandardError: new ComparisonValue<double>(
                    baseline?.StandardError,
                    target?.StandardError
                ),
                Variance: new ComparisonValue<double>(baseline?.Variance, target?.Variance),
                Skewness: new ComparisonValue<double>(baseline?.Skewness, target?.Skewness),
                Kurtosis: new ComparisonValue<double>(baseline?.Kurtosis, target?.Kurtosis),
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
                    target?.BytesAllocatedPerOperation
                ),
                Gen0Collections: new ComparisonValue<int>(
                    baseline?.Gen0Collections,
                    target?.Gen0Collections
                ),
                Gen1Collections: new ComparisonValue<int>(
                    baseline?.Gen1Collections,
                    target?.Gen1Collections
                ),
                Gen2Collections: new ComparisonValue<int>(
                    baseline?.Gen2Collections,
                    target?.Gen2Collections
                ),
                TotalOperations: new ComparisonValue<long>(
                    baseline?.TotalOperations,
                    target?.TotalOperations
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
                N: new ComparisonValue<int>(baseline?.N, target?.N),
                Mean: new ComparisonValue<double>(baseline?.Mean, target?.Mean),
                StandardError: new ComparisonValue<double>(
                    baseline?.StandardError,
                    target?.StandardError
                ),
                Level: new ComparisonValue<int>(baseline?.Level, target?.Level),
                Margin: new ComparisonValue<double>(baseline?.Margin, target?.Margin),
                Lower: new ComparisonValue<double>(baseline?.Lower, target?.Lower),
                Upper: new ComparisonValue<double>(baseline?.Upper, target?.Upper)
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
                P0: new ComparisonValue<double>(baseline?.P0, target?.P0),
                P25: new ComparisonValue<double>(baseline?.P25, target?.P25),
                P50: new ComparisonValue<double>(baseline?.P50, target?.P50),
                P67: new ComparisonValue<double>(baseline?.P67, target?.P67),
                P80: new ComparisonValue<double>(baseline?.P80, target?.P80),
                P85: new ComparisonValue<double>(baseline?.P85, target?.P85),
                P90: new ComparisonValue<double>(baseline?.P90, target?.P90),
                P95: new ComparisonValue<double>(baseline?.P95, target?.P95),
                P100: new ComparisonValue<double>(baseline?.P100, target?.P100)
            );
        }
    }
}

public sealed record ComparisonValue<T>(T? Baseline, T? Target)
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

    public bool IsImprovement(bool lowerIsBetter = true) =>
        Delta.HasValue
        && (lowerIsBetter ? Delta.Value.CompareTo(T.Zero) < 0 : Delta.Value.CompareTo(T.Zero) > 0);

    public bool IsRegression(bool lowerIsBetter = true) =>
        Delta.HasValue
        && (lowerIsBetter ? Delta.Value.CompareTo(T.Zero) > 0 : Delta.Value.CompareTo(T.Zero) < 0);

    public bool HasSignificantChange(double thresholdPercent = 5.0) =>
        PercentageChange.HasValue && Math.Abs(PercentageChange.Value) >= thresholdPercent;

    public string GetChangeSymbol(bool lowerIsBetter = true)
    {
        if (!Delta.HasValue)
            return "?";
        if (Delta.Value.Equals(T.Zero))
            return "=";
        return IsImprovement(lowerIsBetter) ? "✓" : "✗";
    }

    public ConsoleColor GetChangeColor(bool lowerIsBetter = true)
    {
        if (!Delta.HasValue)
            return ConsoleColor.Gray;
        if (Delta.Value.Equals(T.Zero))
            return ConsoleColor.White;
        return IsImprovement(lowerIsBetter) ? ConsoleColor.Green : ConsoleColor.Red;
    }
}
