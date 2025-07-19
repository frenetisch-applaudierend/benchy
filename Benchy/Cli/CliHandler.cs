using System;
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
        Output.EnableVerbose = args.Verbose;

        try
        {
            using var temporaryDirectory = TemporaryDirectory.CreateNew(keep: args.NoDelete);

            var basePath = GetConfigBasePath(args);
            var argsConfig = args.ToConfigFromArgs();
            var config = ConfigurationLoader.LoadConfiguration(
                basePath: basePath,
                argsConfig: argsConfig,
                temporaryDirectory: temporaryDirectory,
                mode: configMode
            );

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

            if (args.NoDelete)
            {
                Output.Info($"Keeping temporary directory {temporaryDirectory.FullName}");
            }
        }
        catch (Exception ex)
        {
            Output.Error(ex.Message);

            if (args.Verbose && ex.StackTrace is { } stackTrace)
            {
                Output.Error(stackTrace);
            }
            Environment.Exit(1);
        }
    }

    protected abstract BenchmarkComparisonResult Handle(TArgs args, ResolvedConfig config);

    protected abstract DirectoryInfo GetConfigBasePath(TArgs args);
}

public abstract record CliHandlerArgs(bool Verbose, bool NoDelete)
{
    public abstract ConfigFromArgs ToConfigFromArgs();
}
