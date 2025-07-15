using System.Text.Json;

namespace Benchy.Core;

public sealed record BenchmarkReport(
    string Title,
    IReadOnlyList<BenchmarkReport.Benchmark> Benchmarks
)
{
    public sealed record Benchmark(string FullName, Statistics Statistics, MemoryMetrics Memory);

    public sealed record Statistics(
        double Mean,
        double Min,
        double Max,
        double Median,
        double StandardDeviation,
        double StandardError,
        double Variance,
        double Skewness,
        double Kurtosis,
        ConfidenceInterval ConfidenceInterval,
        Percentiles Percentiles
    );

    public sealed record MemoryMetrics(
        int BytesAllocatedPerOperation,
        int Gen0Collections,
        int Gen1Collections,
        int Gen2Collections,
        long TotalOperations
    );

    public sealed record ConfidenceInterval(
        int N,
        double Mean,
        double StandardError,
        int Level,
        double Margin,
        double Lower,
        double Upper
    );

    public sealed record Percentiles(
        double P0,
        double P25,
        double P50,
        double P67,
        double P80,
        double P85,
        double P90,
        double P95,
        double P100
    );

    public static IEnumerable<BenchmarkReport> LoadReports(DirectoryInfo directory)
    {
        var reportFiles = directory.GetFiles(
            "*report-full-compressed.json",
            SearchOption.TopDirectoryOnly
        );
        return reportFiles.Select(LoadReport);
    }

    public static BenchmarkReport LoadReport(FileInfo file)
    {
        using var stream = file.OpenRead();

        return JsonSerializer.Deserialize<BenchmarkReport>(stream)
            ?? throw new InvalidOperationException("Failed to deserialize report");
    }
}
