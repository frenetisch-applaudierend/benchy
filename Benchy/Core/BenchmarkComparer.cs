using Benchy.Output;

namespace Benchy.Core;

public static class BenchmarkComparer
{
    public static BenchmarkComparisonResult CompareBenchmarks(
        BenchmarkRun baseline,
        BenchmarkRun target,
        bool verbose,
        double significanceThreshold = 0.05
    )
    {
        BenchmarkRun[] runs = [baseline, target];

        PrepareRuns(runs, verbose);
        var results = RunBenchmarks(runs, verbose);
        var comparisonResult = BenchmarkComparisonResult.FromBenchmarkRunResults(
            results[0], // baseline
            results[1], // target
            significanceThreshold
        );

        return comparisonResult;
    }

    private static void PrepareRuns(IReadOnlyList<BenchmarkRun> runs, bool verbose)
    {
        CliOutput.Info("Preparing benchmarks");

        foreach (var run in runs)
        {
            run.Prepare(verbose);
        }
    }

    private static List<BenchmarkRunResult> RunBenchmarks(BenchmarkRun[] runs, bool verbose)
    {
        CliOutput.Info("Running benchmarks");

        var results = new List<BenchmarkRunResult>(capacity: runs.Length);

        foreach (var run in runs)
        {
            var result = run.Run(verbose);
            results.Add(result);
        }

        return results;
    }
}
