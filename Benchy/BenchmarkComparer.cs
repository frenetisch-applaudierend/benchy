using LibGit2Sharp;

namespace Benchy;

public class BenchmarkComparer
{
    private readonly Repository _repository;

    /// <summary>
    /// Constructor that takes a Repository instance
    /// </summary>
    public BenchmarkComparer(Repository repository)
    {
        _repository = repository ?? throw new ArgumentNullException(nameof(repository));
    }

    /// <summary>
    /// Static factory method to create a BenchmarkComparer instance from a repository path
    /// </summary>
    /// <param name="repositoryPath">Path to the Git repository</param>
    /// <returns>A new BenchmarkComparer instance</returns>
    /// <exception cref="BenchmarkComparisonException">Thrown when there's an error with the repository</exception>
    public static BenchmarkComparer Create(DirectoryInfo repositoryPath)
    {
        if (repositoryPath == null)
            throw new ArgumentNullException(nameof(repositoryPath));

        if (!repositoryPath.Exists)
            throw new BenchmarkComparisonException($"Directory does not exist: {repositoryPath.FullName}");

        try
        {
            var repository = new Repository(repositoryPath.FullName);
            return new BenchmarkComparer(repository);
        }
        catch (RepositoryNotFoundException ex)
        {
            throw new BenchmarkComparisonException($"No Git repository found at: {repositoryPath.FullName}", ex);
        }
        catch (Exception ex)
        {
            throw new BenchmarkComparisonException($"Error opening Git repository: {ex.Message}", ex);
        }
    }

    /// <summary>
    /// Run the benchmark comparison between the two specified commits
    /// </summary>
    /// <param name="baselineCommit">The baseline commit reference (hash, branch, or tag)</param>
    /// <param name="comparisonCommit">The comparison commit reference (hash, branch, or tag)</param>
    /// <exception cref="BenchmarkComparisonException">Thrown when there's an error during comparison</exception>
    public void RunComparison(string baselineCommit, string comparisonCommit)
    {
        if (string.IsNullOrEmpty(baselineCommit))
            throw new ArgumentNullException(nameof(baselineCommit));
            
        if (string.IsNullOrEmpty(comparisonCommit))
            throw new ArgumentNullException(nameof(comparisonCommit));

        try
        {
            Console.WriteLine("Valid Git repository confirmed. Proceeding with benchmark comparison...");
            Console.WriteLine($"Comparing benchmark performance between commits:");
            Console.WriteLine($"  - Baseline: {baselineCommit}");
            Console.WriteLine($"  - Comparison: {comparisonCommit}");
            
            // Validate the commits exist in the repository
            ValidateCommitExists(baselineCommit);
            ValidateCommitExists(comparisonCommit);
            
            // TODO: Implement the full benchmark comparison workflow
            // 1. Check out baseline commit and run benchmark
            // 2. Check out comparison commit and run benchmark
            // 3. Compare and display results
        }
        catch (Exception ex) when (!(ex is BenchmarkComparisonException))
        {
            throw new BenchmarkComparisonException("Error during benchmark comparison", ex);
        }
    }

    /// <summary>
    /// Validates that a commit exists in the repository
    /// </summary>
    /// <param name="commitReference">The commit reference to validate</param>
    /// <exception cref="BenchmarkComparisonException">Thrown when the commit doesn't exist</exception>
    private void ValidateCommitExists(string commitReference)
    {
        try
        {
            var commit = _repository.Lookup<Commit>(commitReference);
            if (commit == null)
            {
                throw new BenchmarkComparisonException($"Commit not found: '{commitReference}'");
            }
        }
        catch (Exception ex) when (!(ex is BenchmarkComparisonException))
        {
            throw new BenchmarkComparisonException($"Error looking up commit '{commitReference}': {ex.Message}", ex);
        }
    }
}
