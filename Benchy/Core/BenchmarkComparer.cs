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
        throw new NotImplementedException("Benchmark comparison logic is not implemented yet.");
    }

    public static void RunAndCompareBenchmarks(
        DirectoryInfo repositoryPath,
        IReadOnlyList<string> commitRefs,
        IReadOnlyList<string> benchmarks,
        bool noDelete,
        bool verbose
    )
    {
        Output.EnableVerbose = verbose;

        if (commitRefs.Count is < 2)
        {
            Output.Error("At least two commit references are required to compare benchmarks");
            Environment.Exit(1);
            return;
        }

        if (benchmarks.Count is 0)
        {
            Output.Error("At least one benchmark must be specified");
            Environment.Exit(1);
            return;
        }

        var deleteAfterRun = !noDelete;
        IReadOnlyList<BenchmarkVersion> versions = [];

        try
        {
            using var repository = GitRepository.Open(repositoryPath.FullName);

            versions =
            [
                .. commitRefs.Select(commitRef =>
                    PrepareComparison(repository, commitRef, benchmarks, verbose)
                ),
            ];
            var results = versions
                .Select(version => RunBenchmarks(version, benchmarks, verbose))
                .ToList();

            AnalyzeBenchmarks(results);
        }
        catch (Exception ex)
        {
            Output.Error(ex.Message);

            if (verbose && ex.StackTrace is { } stackTrace)
            {
                Output.Error(stackTrace);
            }

            Environment.Exit(1);
        }
        finally
        {
            Cleanup(versions, deleteAfterRun);
        }
    }

    private static BenchmarkVersion PrepareComparison(
        GitRepository repository,
        string commitRef,
        IEnumerable<string> benchmarks,
        bool verbose
    )
    {
        Output.Info($"Preparing commit {commitRef}");

        var version = CheckoutVersion(repository, commitRef);

        foreach (var benchmark in benchmarks)
        {
            BuildBenchmark(version, benchmark, verbose);
        }

        return version;
    }

    private static BenchmarkVersion CheckoutVersion(GitRepository repository, string commitRef)
    {
        var version = BenchmarkVersion.CheckoutToTemporaryDirectory(repository, commitRef);
        Output.Info($"Checked out commit {commitRef} to {version.Directory}", indent: 1);
        return version;
    }

    private static void BuildBenchmark(BenchmarkVersion version, string benchmark, bool verbose)
    {
        Output.Info(
            $"Building benchmark project: {benchmark} for commit {version.CommitRef}",
            indent: 1
        );
        var project = version.OpenBenchmarkProject(benchmark);
        project.Build(verbose);
    }

    private static BenchmarkResult RunBenchmarks(
        BenchmarkVersion version,
        IReadOnlyList<string> benchmarks,
        bool verbose
    )
    {
        Output.Info($"Running benchmarks for commit {version.CommitRef}");

        foreach (var benchmark in benchmarks)
        {
            RunBenchmark(version, benchmark, verbose);
        }

        return new BenchmarkResult(
            version,
            [.. BenchmarkReport.LoadReports(version.OutputDirectory.SubDirectory("results"))]
        );
    }

    private static void RunBenchmark(BenchmarkVersion version, string benchmark, bool verbose)
    {
        Output.Info($"Running benchmark: {benchmark} for commit {version.CommitRef}", indent: 1);
        var project = version.OpenBenchmarkProject(benchmark);
        project.Run(
            [
                "--keepFiles",
                "--stopOnFirstError",
                "--memory",
                "--threading",
                "--exporters",
                "JSON",
                "--artifacts",
                version.OutputDirectory.FullName,
            ],
            verbose
        );
    }

    private static void AnalyzeBenchmarks(IReadOnlyList<BenchmarkResult> results)
    {
        Output.Info($"Analyzing benchmark results");

        foreach (var result in results)
        {
            Output.Info($"Analyzing benchmark for commit {result.Version.CommitRef}", indent: 1);
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

    private static void Cleanup(IEnumerable<BenchmarkVersion> versions, bool deleteAfterRun)
    {
        Output.Info("Cleaning up");

        foreach (var version in versions)
        {
            if (deleteAfterRun)
            {
                Output.Verbose($"Deleting checked out commit {version.CommitRef}", indent: 1);
                version.Delete();
            }
            else
            {
                Output.Info(
                    $"Keeping checked out commit {version.CommitRef} at {version.Directory}",
                    indent: 1
                );
            }

            ((IDisposable)version).Dispose();
        }
    }
}
