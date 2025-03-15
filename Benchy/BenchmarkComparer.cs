namespace Benchy;

public static class BenchmarkComparer
{
    public static void RunAndCompareBenchmarks(DirectoryInfo repositoryPath, string baselineCommitRef, string comparisonCommitRef)
    {
        using var repository = GitRepository.Open(repositoryPath.FullName);
        using var baselineVersion = CheckoutVersion(repository, baselineCommitRef);
        using var comparisonVersion = CheckoutVersion(repository, comparisonCommitRef);

        var baselineResults = RunBenchmark(baselineVersion);
        var comparisonResults = RunBenchmark(comparisonVersion);

        CompareBenchmarks(baselineResults, comparisonResults);
    }

    private static BenchmarkVersion CheckoutVersion(GitRepository repository, string commitRef)
    {
        Console.WriteLine($"Checking out commit {commitRef}...");
        return BenchmarkVersion.CheckoutToTemporaryDirectory(repository, commitRef);
    }

    private static BenchmarkResults RunBenchmark(BenchmarkVersion version)
    {
        // TODO: Run the benchmark
        Console.WriteLine("Running benchmark...");

        // TODO: Collect results
        Console.WriteLine($"Deleting checked out commit {version.CommitRef}...");
        version.Delete();

        return new BenchmarkResults();
    }

    private static void CompareBenchmarks(BenchmarkResults baseline, BenchmarkResults comparison)
    {
        // TODO: Compare the results
    }
}

public sealed class BenchmarkResults;
