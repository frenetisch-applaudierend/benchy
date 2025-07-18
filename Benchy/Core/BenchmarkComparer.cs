using Benchy.Infrastructure;

namespace Benchy.Core;

public static class BenchmarkComparer
{
    public static BenchmarkComparisonResult CompareBenchmarks(
        BenchmarkRun baseline,
        BenchmarkRun target,
        bool verbose
    )
    {
        BenchmarkRun[] runs = [baseline, target];

        PrepareRuns(runs, verbose);
        var results = RunBenchmarks(runs, verbose);
        var comparisonResult = BenchmarkComparisonResult.FromBenchmarkRunResults(
            results[0], // baseline
            results[1] // target
        );

        return comparisonResult;
    }

    private static void PrepareRuns(IReadOnlyList<BenchmarkRun> runs, bool verbose)
    {
        foreach (var run in runs)
        {
            Output.Info($"Preparing benchmarks for {run.Name}");
            run.Prepare(verbose);
        }
    }

    private static IReadOnlyList<BenchmarkRunResult> RunBenchmarks(
        BenchmarkRun[] runs,
        bool verbose
    )
    {
        var results = new List<BenchmarkRunResult>(capacity: runs.Length);

        foreach (var run in runs)
        {
            var result = run.Run(verbose);
            results.Add(result);
        }

        return results;
    }
}
