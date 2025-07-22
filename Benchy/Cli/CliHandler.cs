using Benchy.Configuration;
using Benchy.Core;
using Benchy.Infrastructure;
using Benchy.Output;
using Benchy.Reporting;

namespace Benchy.Cli;

public abstract class CliHandler<TArgs>(ConfigurationLoader.Mode configMode)
    where TArgs : CliHandlerArgs
{
    public void Handle(TArgs args)
    {
        bool verboseOutput = args.Verbose ?? false;

        try
        {
            CliOutput.EnableVerbose = verboseOutput;
            CliOutput.Writer = new ConsoleOutputWriter(
                interactive: args.Decoration ?? configMode == ConfigurationLoader.Mode.Interactive
            );

            using var temporaryDirectory = TemporaryDirectory.CreateNew();

            var basePath = GetConfigBasePath(args);
            var argsConfig = GetConfigFromArgs(args);
            var config = ConfigurationLoader.LoadConfiguration(
                basePath: basePath,
                argsConfig: argsConfig,
                temporaryDirectory: temporaryDirectory,
                mode: configMode
            );

            verboseOutput = config.Verbose;
            CliOutput.EnableVerbose = config.Verbose;
            CliOutput.Writer = new ConsoleOutputWriter(interactive: config.Decoration);

            if (config.NoDelete)
            {
                temporaryDirectory.KeepAfterDisposal();
            }

            PrintConfiguration(config);

            if (config.Benchmarks.Length == 0)
            {
                throw new ArgumentException(
                    "At least one benchmark must be specified (via command line or configuration file)."
                );
            }

            var results = Handle(args, config);

            var reporter = ReporterFactory.CreateReporter(
                config.OutputStyle,
                config.OutputDirectory
            );
            reporter.GenerateReport(results);

            if (config.NoDelete)
            {
                CliOutput.Info($"Keeping temporary directory {temporaryDirectory.FullName}");
            }
        }
        catch (Exception ex)
        {
            CliOutput.Error(ex.Message);

            if (verboseOutput && ex.StackTrace is { } stackTrace)
            {
                CliOutput.Error(stackTrace);
            }
            Environment.Exit(1);
        }
    }

    private static void PrintConfiguration(ResolvedConfig config)
    {
        CliOutput.Verbose("Resolved Configuration:");
        CliOutput.Verbose($"  Output Directory: {config.OutputDirectory.FullName}");
        CliOutput.Verbose($"  Verbose: {config.Verbose}");
        CliOutput.Verbose($"  Output Style: {string.Join(", ", config.OutputStyle)}");
        CliOutput.Verbose($"  Benchmarks: {string.Join(", ", config.Benchmarks)}");
        CliOutput.Verbose($"  No Delete: {config.NoDelete}");
        CliOutput.Verbose($"  Decoration: {config.Decoration}");
    }

    private static ConfigFromArgs GetConfigFromArgs(TArgs args)
    {
        return new()
        {
            Verbose = args.Verbose,
            OutputDirectory = args.OutputDirectory,
            OutputStyle = args.OutputStyle,
            Benchmarks = args.Benchmarks,
            NoDelete = args.NoDelete,
            SignificanceThreshold = args.SignificanceThreshold,
            Decoration = args.Decoration,
        };
    }

    protected abstract BenchmarkComparisonResult Handle(TArgs args, ResolvedConfig config);

    protected abstract DirectoryInfo GetConfigBasePath(TArgs args);
}

public class CliHandlerArgs
{
    public required bool? Verbose { get; init; }
    public required bool? NoDelete { get; init; }
    public required string? OutputDirectory { get; init; }
    public required string[]? OutputStyle { get; init; }
    public required string[]? Benchmarks { get; init; }
    public required double? SignificanceThreshold { get; init; }
    public required bool? Decoration { get; init; }
}
