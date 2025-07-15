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
        Output.EnableVerbose = verbose;

        BenchmarkRun[] runs = [baseline, target];

        try
        {
            PrepareRuns(runs, verbose);
            var results = RunBenchmarks(runs, verbose);
            AnalyzeBenchmarks(results);

            return new BenchmarkComparisonResult();
        }
        catch (Exception ex)
        {
            return Output.Fail<BenchmarkComparisonResult>(ex, verbose);
        }
    }

    private static void PrepareRuns(IReadOnlyList<BenchmarkRun> runs, bool verbose)
    {
        foreach (var run in runs)
        {
            Output.Info($"Preparing benchmarks for {run.Name}");
            run.Prepare(verbose);
        }
    }

    private static IReadOnlyList<BenchmarkResult> RunBenchmarks(BenchmarkRun[] runs, bool verbose)
    {
        var results = new List<BenchmarkResult>(capacity: runs.Length);

        foreach (var run in runs)
        {
            var result = run.Run(verbose);
            results.Add(result);
        }

        return results;
    }

    private static void AnalyzeBenchmarks(IReadOnlyList<BenchmarkResult> results)
    {
        Output.Info($"Analyzing benchmark results");

        foreach (var result in results)
        {
            Output.Info($"Analyzing benchmark for run {result.Run.Name}", indent: 1);
            foreach (var report in result.Reports)
            {
                Output.Info($"Report: {report.Title}", indent: 2);
                foreach (var benchmark in report.Benchmarks)
                {
                    Output.Info(
                        $"Benchmark: {benchmark.FullName} - Mean: {benchmark.Statistics.Mean}",
                        indent: 3
                    );
                }
            }
        }
    }
}
