using Benchy.Configuration;
using Benchy.Core;
using Benchy.Infrastructure;
using Benchy.Infrastructure.Reporting;

namespace Benchy.Cli;

public abstract class CliHandler<TArgs>(ConfigurationLoader.Mode configMode)
    where TArgs : CliHandlerArgs
{
    public void Handle(TArgs args)
    {
        bool verboseOutput = args.Verbose ?? false;

        try
        {
            using var temporaryDirectory = TemporaryDirectory.CreateNew();

            var basePath = GetConfigBasePath(args);
            var argsConfig = GetConfigFromArgs(args);
            var config = ConfigurationLoader.LoadConfiguration(
                basePath: basePath,
                argsConfig: argsConfig,
                temporaryDirectory: temporaryDirectory,
                mode: configMode
            );

            Output.EnableVerbose = config.Verbose;
            verboseOutput = config.Verbose;
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

            var reporter = Reporting.CreateReporter(
                config.OutputStyle,
                config.OutputDirectory,
                Console.Out,
                useColors: true,
                isInteractiveMode: configMode == ConfigurationLoader.Mode.Interactive
            );
            reporter.GenerateReport(results);

            if (config.NoDelete)
            {
                Output.Info($"Keeping temporary directory {temporaryDirectory.FullName}");
            }
        }
        catch (Exception ex)
        {
            Output.Error(ex.Message);

            if (verboseOutput && ex.StackTrace is { } stackTrace)
            {
                Output.Error(stackTrace);
            }
            Environment.Exit(1);
        }
    }

    private static void PrintConfiguration(ResolvedConfig config)
    {
        Output.Verbose("Resolved Configuration:");
        Output.Verbose($"  Output Directory: {config.OutputDirectory.FullName}");
        Output.Verbose($"  Verbose: {config.Verbose}");
        Output.Verbose($"  Output Style: {string.Join(", ", config.OutputStyle)}");
        Output.Verbose($"  Benchmarks: {string.Join(", ", config.Benchmarks)}");
        Output.Verbose($"  No Delete: {config.NoDelete}");
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
}
