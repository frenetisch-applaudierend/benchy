namespace Benchy.Core;

public sealed record BenchmarkRunResult(BenchmarkRun Run, IReadOnlyList<BenchmarkReport> Reports);
