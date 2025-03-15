namespace Benchy;

public static class BenchmarkComparer
{
    public static void RunAndCompareBenchmarks(DirectoryInfo repositoryPath, string baselineCommitRef, string comparisonCommitRef)
    {
        using var repository = GitRepository.Open(repositoryPath.FullName);
        using var baselineVersion = BenchmarkVersion.CheckoutToTemporaryDirectory(repository, baselineCommitRef);
        using var comparisonVersion = BenchmarkVersion.CheckoutToTemporaryDirectory(repository, comparisonCommitRef);

        var baselineResults = RunBenchmark(baselineVersion);
        var comparisonResults = RunBenchmark(comparisonVersion);

        CompareBenchmarks(baselineResults, comparisonResults);
    }

    private static BenchmarkResults RunBenchmark(BenchmarkVersion version)
    {
        // TODO: Run the benchmark
        // TODO: Collect results

        version.Delete();

        return new BenchmarkResults();
    }

    private static void CompareBenchmarks(BenchmarkResults baseline, BenchmarkResults comparison)
    {
        // TODO: Compare the results
    }
}

public sealed class BenchmarkResults;
