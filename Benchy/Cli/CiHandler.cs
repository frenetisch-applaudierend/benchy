using Benchy.Configuration;
using Benchy.Core;
using Benchy.Infrastructure;
using Benchy.Infrastructure.Reporting;

namespace Benchy.Cli;

public class CiHandler() : CliHandler<CiHandler.Args>(ConfigurationLoader.Mode.Ci)
{
    public sealed record Args(
        bool Verbose,
        bool NoDelete,
        string? OutputDirectory,
        string[]? OutputStyle,
        string[]? Benchmarks,
        DirectoryInfo BaselineDirectory,
        DirectoryInfo TargetDirectory
    ) : CliHandlerArgs(Verbose, NoDelete)
    {
        public override ConfigFromArgs ToConfigFromArgs() =>
            new()
            {
                Verbose = Verbose,
                OutputDirectory = OutputDirectory,
                OutputStyle = OutputStyle,
                Benchmarks = Benchmarks,
                NoDelete = NoDelete,
            };
    }

    public static void Handle(
        bool verbose,
        DirectoryInfo? providedOutputDirectory,
        string[] outputStyles,
        string[] benchmarks,
        DirectoryInfo baselineDirectory,
        DirectoryInfo targetDirectory
    )
    {
        var args = new Args(
            Verbose: verbose,
            NoDelete: true,
            OutputDirectory: providedOutputDirectory?.FullName,
            OutputStyle: outputStyles,
            Benchmarks: benchmarks,
            BaselineDirectory: baselineDirectory,
            TargetDirectory: targetDirectory
        );
        var handler = new CiHandler();

        handler.Handle(args);
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
