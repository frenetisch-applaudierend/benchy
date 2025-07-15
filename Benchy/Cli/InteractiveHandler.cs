using Benchy.Core;
using Benchy.Infrastructure;

namespace Benchy.Cli;

public static class InteractiveHandler
{
    public static void Handle(
        bool verbose,
        string[] benchmarks,
        DirectoryInfo? repositoryPath,
        bool noDelete,
        string baselineRef,
        string? targetRef
    )
    {
        var repository = FindRepository(repositoryPath);

        var baselineTemporaryDirectory = Directories.CreateTemporaryDirectory("baseline");
        var baselineSourceDirectory = CheckoutReference(
            repository: repository,
            reference: baselineRef,
            temporaryDirectory: baselineTemporaryDirectory
        );
        var baselineRun = BenchmarkRun.FromSourcePath(
            sourceDirectory: baselineSourceDirectory,
            temporaryDirectory: baselineTemporaryDirectory,
            name: $"baseline ({baselineRef})",
            benchmarks: benchmarks
        );

        var targetTemporaryDirectory = Directories.CreateTemporaryDirectory("target");
        var targetSourceDirectory = CheckoutReference(
            repository: repository,
            reference: targetRef,
            temporaryDirectory: targetTemporaryDirectory
        );
        var targetRun = BenchmarkRun.FromSourcePath(
            sourceDirectory: targetSourceDirectory,
            temporaryDirectory: targetTemporaryDirectory,
            name: $"target ({targetRef ?? "HEAD"})",
            benchmarks: benchmarks
        );

        var results = BenchmarkComparer.CompareBenchmarks(baselineRun, targetRun, verbose);

        _ = results; // TODO

        if (!noDelete)
        {
            Output.Info("Cleaning up temporary directories");
            baselineTemporaryDirectory.Delete(true);
            targetTemporaryDirectory.Delete(true);
        }
        else
        {
            Output.Info("Skipping cleanup of temporary directories");
        }
    }

    private static GitRepository FindRepository(DirectoryInfo? repositoryPath)
    {
        if (repositoryPath is not null)
        {
            return GitRepository.Open(repositoryPath.FullName);
        }

        // Auto-detect repository from current directory
        return GitRepository.Open(Directory.GetCurrentDirectory());
    }

    private static DirectoryInfo CheckoutReference(
        GitRepository repository,
        string? reference,
        DirectoryInfo temporaryDirectory
    )
    {
        if (string.IsNullOrEmpty(reference))
        {
            throw new NotImplementedException("No target reference is not implemented yet.");
        }

        Output.Info($"Checking out reference '{reference}'");

        var sourceDirectory = temporaryDirectory.CreateSubdirectory("src");

        GitRepository.Clone(repository, sourceDirectory.FullName).Checkout(reference);

        return sourceDirectory;
    }
}
