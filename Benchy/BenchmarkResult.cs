namespace Benchy;

public record BenchmarkResult(BenchmarkVersion Version, IReadOnlyList<BenchmarkReport> Reports);
