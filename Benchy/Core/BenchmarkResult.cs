namespace Benchy.Core;

public record BenchmarkResult(BenchmarkRun Run, IReadOnlyList<BenchmarkReport> Reports);
