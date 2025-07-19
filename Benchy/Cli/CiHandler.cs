using Benchy.Configuration;
using Benchy.Core;

namespace Benchy.Cli;

public class CiHandler() : CliHandler<CiHandler.Args>(ConfigurationLoader.Mode.Ci)
{
    public sealed class Args : CliHandlerArgs
    {
        public required DirectoryInfo BaselineDirectory { get; init; }
        public required DirectoryInfo TargetDirectory { get; init; }
    }

    public static void Handle(
        bool? verbose,
        DirectoryInfo? providedOutputDirectory,
        string[]? outputStyles,
        string[]? benchmarks,
        DirectoryInfo baselineDirectory,
        DirectoryInfo targetDirectory
    )
    {
        new CiHandler().Handle(
            new Args
            {
                Verbose = verbose,
                NoDelete = true,
                OutputDirectory = providedOutputDirectory?.FullName,
                OutputStyle = outputStyles,
                Benchmarks = benchmarks,
                BaselineDirectory = baselineDirectory,
                TargetDirectory = targetDirectory,
            }
        );
    }

    protected override BenchmarkComparisonResult Handle(Args args, ResolvedConfig config)
    {
        var baselineRun = BenchmarkRun.FromSourcePath(
            sourceDirectory: args.BaselineDirectory,
            outputDirectory: config.OutputDirectory.CreateSubdirectory("baseline"),
            name: "baseline",
            benchmarks: config.Benchmarks
        );

        var targetRun = BenchmarkRun.FromSourcePath(
            sourceDirectory: args.TargetDirectory,
            outputDirectory: config.OutputDirectory.CreateSubdirectory("target"),
            name: "target",
            benchmarks: config.Benchmarks
        );

        return BenchmarkComparer.CompareBenchmarks(baselineRun, targetRun, config.Verbose);
    }

    protected override DirectoryInfo GetConfigBasePath(Args args)
    {
        return args.TargetDirectory;
    }
}
