using Benchy.Core;
using Benchy.Infrastructure;
using Benchy.Infrastructure.Reporting;

namespace Benchy.Cli;

public static class InteractiveHandler
{
    public static void Handle(
        bool verbose,
        DirectoryInfo? providedOutputDirectory,
        string[] outputStyles,
        string[] benchmarks,
        DirectoryInfo? repositoryPath,
        bool noDelete,
        string baselineRef,
        string? targetRef
    )
    {
        Output.EnableVerbose = verbose;

        try
        {
            HandleInternal(
                verbose,
                providedOutputDirectory,
                outputStyles,
                benchmarks,
                repositoryPath,
                noDelete,
                baselineRef,
                targetRef
            );
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
    }

    private static void HandleInternal(
        bool verbose,
        DirectoryInfo? providedOutputDirectory,
        string[] outputStyles,
        string[] benchmarks,
        DirectoryInfo? repositoryPath,
        bool noDelete,
        string baselineRef,
        string? targetRef
    )
    {
        using var temporaryDirectory = TemporaryDirectory.CreateNew(keep: noDelete);
        Output.Verbose($"Temporary directory for comparison: {temporaryDirectory.FullName}");

        var outputDirectory =
            providedOutputDirectory ?? temporaryDirectory.CreateSubdirectory("out");
        var sourceDirectory = temporaryDirectory.CreateSubdirectory("src");

        var repository = FindRepository(repositoryPath);

        var baselineRun = CheckoutRun(
            repository: repository,
            label: "baseline",
            reference: baselineRef,
            checkoutRootDirectory: sourceDirectory,
            outputDirectory: outputDirectory,
            benchmarks: benchmarks
        );

        var targetRun = CheckoutRun(
            repository: repository,
            label: "target",
            reference: targetRef,
            checkoutRootDirectory: sourceDirectory,
            outputDirectory: outputDirectory,
            benchmarks: benchmarks
        );

        var results = BenchmarkComparer.CompareBenchmarks(baselineRun, targetRun, verbose);

        var reporter = Reporting.CreateReporter(
            outputStyles,
            outputDirectory,
            Console.Out,
            useColors: true,
            isInteractiveMode: true
        );
        reporter.GenerateReport(results);

        if (noDelete)
        {
            Output.Info(
                $"Skipping cleanup of temporary directories at {temporaryDirectory.FullName}"
            );
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

    private static BenchmarkRun CheckoutRun(
        GitRepository repository,
        string label,
        string? reference,
        DirectoryInfo checkoutRootDirectory,
        DirectoryInfo outputDirectory,
        string[] benchmarks
    )
    {
        DirectoryInfo runSourceDirectory;

        if (string.IsNullOrEmpty(reference))
        {
            Output.Info($"Using current working copy for {label}");
            if (repository.WorkingDirectory is not { } workingDirectory)
            {
                throw new InvalidOperationException(
                    "Comparing to the working copy is not supported for bare repositories"
                );
            }

            runSourceDirectory = workingDirectory;
        }
        else
        {
            Output.Info($"Checking out reference '{reference}' for {label}");
            runSourceDirectory = checkoutRootDirectory.CreateSubdirectory(label);
            GitRepository.Clone(repository, runSourceDirectory.FullName).Checkout(reference);
        }

        return BenchmarkRun.FromSourcePath(
            sourceDirectory: runSourceDirectory,
            outputDirectory: outputDirectory.CreateSubdirectory(label),
            name: $"{label} ({reference ?? "working copy"})",
            benchmarks: benchmarks
        );
    }
}
