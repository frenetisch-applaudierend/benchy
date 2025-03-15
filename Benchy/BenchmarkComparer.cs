namespace Benchy;

public static class BenchmarkComparer
{
    public static void RunAndCompareBenchmarks(DirectoryInfo repositoryPath, string baselineCommitRef, string comparisonCommitRef, bool noDelete)
    {
        var deleteAfterRun = !noDelete;
        using var repository = GitRepository.Open(repositoryPath.FullName);
        using var baselineVersion = CheckoutVersion(repository, baselineCommitRef);
        using var comparisonVersion = CheckoutVersion(repository, comparisonCommitRef);

        var baselineResults = RunBenchmark(baselineVersion, deleteAfterRun);
        var comparisonResults = RunBenchmark(comparisonVersion, deleteAfterRun);

        CompareBenchmarks(baselineResults, comparisonResults);
    }

    private static BenchmarkVersion CheckoutVersion(GitRepository repository, string commitRef)
    {
        var version = BenchmarkVersion.CheckoutToTemporaryDirectory(repository, commitRef);
        Console.WriteLine($"Checked out commit {commitRef} to {version.Directory}");
        return version;
    }

    private static BenchmarkResults RunBenchmark(BenchmarkVersion version, bool deleteAfterRun)
    {
        // TODO: Run the benchmark
        Console.WriteLine("Running benchmark...");

        // TODO: Collect results

        if (deleteAfterRun)
        {
            Console.WriteLine($"Deleting checked out commit {version.CommitRef}");
            version.Delete();
        }
        else
        {
            Console.WriteLine($"Keeping checked out commit {version.CommitRef} at {version.Directory}");
        }


        return new BenchmarkResults();
    }

    private static void CompareBenchmarks(BenchmarkResults baseline, BenchmarkResults comparison)
    {
        // TODO: Compare the results
    }
}

public sealed class BenchmarkResults;
