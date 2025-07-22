using Benchy.Configuration;
using Benchy.Core;
using Benchy.Infrastructure;
using Benchy.Output;
using static Benchy.Output.FormattedText;

namespace Benchy.Cli;

public class InteractiveHandler()
    : CliHandler<InteractiveHandler.Args>(ConfigurationLoader.Mode.Interactive)
{
    public class Args : CliHandlerArgs
    {
        public required DirectoryInfo? RepositoryPath { get; init; }
        public required string BaselineRef { get; init; }
        public required string? TargetRef { get; init; }
    }

    public static void Handle(
        bool? verbose,
        DirectoryInfo? providedOutputDirectory,
        string[]? outputStyles,
        string[]? benchmarks,
        DirectoryInfo? repositoryPath,
        bool? noDelete,
        string baselineRef,
        string? targetRef
    )
    {
        new InteractiveHandler().Handle(
            new Args
            {
                Verbose = verbose,
                NoDelete = noDelete,
                OutputDirectory = providedOutputDirectory?.FullName,
                OutputStyle = outputStyles,
                Benchmarks = benchmarks,
                RepositoryPath = repositoryPath,
                BaselineRef = baselineRef,
                TargetRef = targetRef,
            }
        );
    }

    protected override BenchmarkComparisonResult Handle(Args args, ResolvedConfig config)
    {
        CliOutput.Info("Setting up benchmark versions");

        var sourceDirectory = config.TemporaryDirectory.CreateSubdirectory("src");
        var repository = FindRepository(args.RepositoryPath);

        var baselineRun = CheckoutRun(
            repository: repository,
            label: "baseline",
            reference: args.BaselineRef,
            checkoutRootDirectory: sourceDirectory,
            outputDirectory: config.OutputDirectory,
            benchmarks: config.Benchmarks
        );

        var targetRun = CheckoutRun(
            repository: repository,
            label: "target",
            reference: args.TargetRef,
            checkoutRootDirectory: sourceDirectory,
            outputDirectory: config.OutputDirectory,
            benchmarks: config.Benchmarks
        );

        return BenchmarkComparer.CompareBenchmarks(baselineRun, targetRun, config.Verbose);
    }

    protected override DirectoryInfo GetConfigBasePath(Args args)
    {
        return args.RepositoryPath ?? new DirectoryInfo(Directory.GetCurrentDirectory());
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
            CliOutput.Info(
                $"{Decor("🏷️  ")}Using {Em("current working copy")} for {Em(label)}",
                indent: 1
            );
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
            CliOutput.Info(
                $"{Decor("🏷️  ")}Checking out reference {Em(reference)} for {Em(label)}",
                indent: 1
            );
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
