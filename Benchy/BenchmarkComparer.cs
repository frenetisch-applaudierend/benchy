using LibGit2Sharp;
using System.Diagnostics;
using System.IO;

namespace Benchy;

public class BenchmarkComparer
{
    private readonly Repository _repository;
    private readonly string _repositoryPath;

    /// <summary>
    /// Constructor that takes a Repository instance
    /// </summary>
    public BenchmarkComparer(Repository repository)
    {
        _repository = repository ?? throw new ArgumentNullException(nameof(repository));
        _repositoryPath = _repository.Info.WorkingDirectory;
    }

    /// <summary>
    /// Static factory method to create a BenchmarkComparer instance from a repository path
    /// </summary>
    /// <param name="repositoryPath">Path to the Git repository</param>
    /// <returns>A new BenchmarkComparer instance</returns>
    /// <exception cref="BenchmarkComparisonException">Thrown when there's an error with the repository</exception>
    public static BenchmarkComparer Create(DirectoryInfo repositoryPath)
    {
        ArgumentNullException.ThrowIfNull(repositoryPath);

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
        ArgumentNullException.ThrowIfNullOrWhiteSpace(baselineCommit, nameof(baselineCommit));
        ArgumentNullException.ThrowIfNullOrWhiteSpace(comparisonCommit, nameof(comparisonCommit));

        // Create temporary directories
        string baselineTempDir = CreateTempDirectory("baseline");
        string comparisonTempDir = CreateTempDirectory("comparison");

        try
        {
            Console.WriteLine("Valid Git repository confirmed. Proceeding with benchmark comparison...");
            Console.WriteLine($"Comparing benchmark performance between commits:");
            Console.WriteLine($"  - Baseline: {baselineCommit}");
            Console.WriteLine($"  - Comparison: {comparisonCommit}");

            // Validate the commits exist in the repository
            ValidateCommitExists(baselineCommit);
            ValidateCommitExists(comparisonCommit);

            // Clone repo at specific commits to temporary directories
            Console.WriteLine($"Cloning baseline commit to {baselineTempDir}...");
            ShallowCloneAtCommit(_repositoryPath, baselineCommit, baselineTempDir);

            Console.WriteLine($"Cloning comparison commit to {comparisonTempDir}...");
            ShallowCloneAtCommit(_repositoryPath, comparisonCommit, comparisonTempDir);

            // TODO: Run benchmarks in each directory
            // TODO: Compare results

            Console.WriteLine("Benchmark comparison completed!");
        }
        catch (Exception ex) when (!(ex is BenchmarkComparisonException))
        {
            throw new BenchmarkComparisonException("Error during benchmark comparison", ex);
        }
        finally
        {
            // Clean up temporary directories
            CleanupTempDirectory(baselineTempDir);
            CleanupTempDirectory(comparisonTempDir);
        }
    }

    /// <summary>
    /// Performs a shallow clone of the repository at a specific commit
    /// </summary>
    /// <param name="sourceRepoPath">Path to the source repository</param>
    /// <param name="commitRef">Commit reference to clone</param>
    /// <param name="targetDirectory">Directory to clone to</param>
    private static void ShallowCloneAtCommit(string sourceRepoPath, string commitRef, string targetDirectory)
    {
        try
        {
            // Use the git CLI for shallow cloning at a specific commit
            // This is more efficient than what LibGit2Sharp can do 
            ProcessStartInfo startInfo = new ProcessStartInfo
            {
                FileName = "git",
                Arguments = $"clone --depth 1 --branch {commitRef} --single-branch \"{sourceRepoPath}\" \"{targetDirectory}\"",
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                UseShellExecute = false,
                CreateNoWindow = true
            };

            using Process process = Process.Start(startInfo) ?? throw new BenchmarkComparisonException("Failed to start git clone process");
            string output = process.StandardOutput.ReadToEnd();
            string error = process.StandardError.ReadToEnd();
            process.WaitForExit();

            if (process.ExitCode != 0)
            {
                // If branching fails (likely because commitRef is a commit hash, not a branch), try direct checkout
                // First clone the repo without specifying branch
                ProcessStartInfo cloneInfo = new ProcessStartInfo
                {
                    FileName = "git",
                    Arguments = $"clone --no-checkout \"{sourceRepoPath}\" \"{targetDirectory}\"",
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    UseShellExecute = false,
                    CreateNoWindow = true
                };

                using Process cloneProcess = Process.Start(cloneInfo) ?? throw new BenchmarkComparisonException("Failed to start git clone process");
                cloneProcess.WaitForExit();

                if (cloneProcess.ExitCode != 0)
                {
                    throw new BenchmarkComparisonException($"Failed to clone repository: {cloneProcess.StandardError.ReadToEnd()}");
                }

                // Then checkout the specific commit
                ProcessStartInfo checkoutInfo = new ProcessStartInfo
                {
                    FileName = "git",
                    Arguments = $"checkout {commitRef}",
                    WorkingDirectory = targetDirectory,
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    UseShellExecute = false,
                    CreateNoWindow = true
                };

                using Process checkoutProcess = Process.Start(checkoutInfo) ?? throw new BenchmarkComparisonException("Failed to start git checkout process");
                checkoutProcess.WaitForExit();

                if (checkoutProcess.ExitCode != 0)
                {
                    throw new BenchmarkComparisonException($"Failed to checkout commit: {checkoutProcess.StandardError.ReadToEnd()}");
                }

                // Finally, remove the .git directory to save space
                string gitDir = Path.Combine(targetDirectory, ".git");
                if (Directory.Exists(gitDir))
                {
                    Directory.Delete(gitDir, true);
                }
            }
        }
        catch (Exception ex) when (!(ex is BenchmarkComparisonException))
        {
            throw new BenchmarkComparisonException($"Failed to clone repository at commit {commitRef}: {ex.Message}", ex);
        }
    }

    /// <summary>
    /// Gets a commit object from a commit reference
    /// </summary>
    /// <param name="commitReference">The commit reference to lookup</param>
    /// <returns>The Commit object</returns>
    /// <exception cref="BenchmarkComparisonException">Thrown when the commit doesn't exist</exception>
    private Commit GetCommitObject(string commitReference)
    {
        try
        {
            return _repository.Lookup<Commit>(commitReference) ?? throw new BenchmarkComparisonException($"Commit not found: '{commitReference}'");
        }
        catch (Exception ex) when (!(ex is BenchmarkComparisonException))
        {
            throw new BenchmarkComparisonException($"Error looking up commit '{commitReference}': {ex.Message}", ex);
        }
    }

    /// <summary>
    /// Creates a temporary directory for checking out a commit
    /// </summary>
    /// <param name="prefix">Prefix for the directory name</param>
    /// <returns>Path to the temporary directory</returns>
    private static string CreateTempDirectory(string prefix)
    {
        string tempDir = Path.Combine(Path.GetTempPath(), $"benchy_{prefix}_{Path.GetRandomFileName()}");
        Directory.CreateDirectory(tempDir);
        return tempDir;
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
        catch (Exception ex) when (ex is not BenchmarkComparisonException)
        {
            throw new BenchmarkComparisonException($"Error looking up commit '{commitReference}': {ex.Message}", ex);
        }
    }

    /// <summary>
    /// Cleans up a temporary directory
    /// </summary>
    /// <param name="directory">The directory to clean up</param>
    private static void CleanupTempDirectory(string directory)
    {
        try
        {
            if (Directory.Exists(directory))
            {
                Console.WriteLine($"Cleaning up temporary directory: {directory}");
                //Directory.Delete(directory, true);
            }
        }
        catch (Exception ex)
        {
            // Just log the error but don't throw, as this is cleanup code
            Console.Error.WriteLine($"Warning: Failed to clean up temporary directory {directory}: {ex.Message}");
        }
    }
}
