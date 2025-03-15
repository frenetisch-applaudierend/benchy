namespace Benchy;

public static class BenchmarkComparer
{
    public static void RunBenchmarks(DirectoryInfo repositoryPath, string baselineCommitRef, string comparisonCommitRef)
    {
        using var repository = GitRepository.Open(repositoryPath.FullName);
        using var baselineVersion = BenchmarkVersion.CheckoutToTemporaryDirectory(repository, baselineCommitRef);
        using var comparisonVersion = BenchmarkVersion.CheckoutToTemporaryDirectory(repository, comparisonCommitRef);

        RunBenchmark(baselineVersion);
        RunBenchmark(comparisonVersion);

        CompareBenchmarks(baselineVersion, comparisonVersion);
    }

    private static void RunBenchmark(BenchmarkVersion version)
    {
        throw new NotImplementedException();
    }

    private static void CompareBenchmarks(BenchmarkVersion baseline, BenchmarkVersion comparison)
    {
        throw new NotImplementedException();
    }
}
