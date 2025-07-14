namespace Benchy.Core;

public record BenchmarkResult(BenchmarkVersion Version, IReadOnlyList<BenchmarkReport> Reports);
