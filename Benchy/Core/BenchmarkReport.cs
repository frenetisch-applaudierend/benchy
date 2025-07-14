using System.Text.Json;
using Benchy.Infrastructure;

namespace Benchy.Core;

public sealed record BenchmarkReport(
    string Title,
    IReadOnlyList<BenchmarkReport.Benchmark> Benchmarks
)
{
    public sealed record Benchmark(string FullName, Statistics Statistics);

    public sealed record Statistics(double Mean);

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
