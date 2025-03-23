namespace Benchy;

public static class BenchmarkComparer
{
    public static void RunAndCompareBenchmarks(
        DirectoryInfo repositoryPath,
        IReadOnlyList<string> commitRefs,
        IReadOnlyList<string> benchmarks,
        bool noDelete,
        bool verbose)
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

            versions = [.. commitRefs.Select(commitRef => PrepareComparison(repository, commitRef, benchmarks, verbose))];
            IReadOnlyList<BenchmarkResults> results = [.. versions.Select(version => RunBenchmarks(version, benchmarks, verbose))];

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

    private static BenchmarkVersion PrepareComparison(GitRepository repository, string commitRef, IEnumerable<string> benchmarks, bool verbose)
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
        Output.Info($"Building benchmark: {benchmark} for commit {version.CommitRef}", indent: 1);
        var project = version.OpenBenchmarkProject(benchmark);
        project.Build(verbose);
    }

    private static BenchmarkResults RunBenchmarks(BenchmarkVersion version, IReadOnlyList<string> benchmarks, bool verbose)
    {

        Output.Info($"Running benchmarks for commit {version.CommitRef}");

        foreach (var benchmark in benchmarks)
        {
            RunBenchmark(version, benchmark, verbose);
        }

        // TODO: Collect results

        return new BenchmarkResults();
    }

    private static void RunBenchmark(BenchmarkVersion version, string benchmark, bool verbose)
    {
        Output.Info($"Running benchmark: {benchmark} for commit {version.CommitRef}", indent: 1);
        var project = version.OpenBenchmarkProject(benchmark);
        project.Run(
        [
            "--keepFiles",
            "--stopOnFirstError",
            "--exporters",
            "JSON",
            "--artifacts",
            version.OutputDirectory.FullName
        ], verbose);
    }

    private static void AnalyzeBenchmarks(IEnumerable<BenchmarkResults> results)
    {
        Output.Info($"Analyzing benchmark results");
        // TODO: Compare the results
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
                Output.Info($"Keeping checked out commit {version.CommitRef} at {version.Directory}", indent: 1);
            }

            ((IDisposable)version).Dispose();
        }
    }
}

public sealed class BenchmarkResults;
