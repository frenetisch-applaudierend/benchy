using Benchy.Configuration;
using Benchy.Core;
using Benchy.Infrastructure;
using Benchy.Infrastructure.Reporting;

namespace Benchy.Cli;

public static class CiHandler
{
    public static void Handle(
        bool verbose,
        DirectoryInfo? providedOutputDirectory,
        string[] outputStyles,
        string[] benchmarks,
        DirectoryInfo baselineDirectory,
        DirectoryInfo targetDirectory
    )
    {
        try
        {
            // Load and resolve configuration
            var fileConfig = ConfigurationLoader.LoadConfiguration();
            var resolvedConfig = ConfigurationLoader.ResolveConfiguration(
                fileConfig,
                isInteractiveMode: false,
                verboseOverride: verbose,
                outputDirectoryOverride: providedOutputDirectory,
                outputStyleOverride: outputStyles.Length > 0 ? outputStyles : null,
                benchmarksOverride: benchmarks.Length > 0 ? benchmarks : null
            );

            Output.EnableVerbose = resolvedConfig.Verbose;

            if (resolvedConfig.Benchmarks.Length == 0)
            {
                throw new ArgumentException(
                    "At least one benchmark must be specified (via command line or configuration file)."
                );
            }

            using var temporaryDirectory = TemporaryDirectory.CreateNew(keep: true);
            Output.Verbose($"Temporary directory for comparison: {temporaryDirectory.FullName}");

            var outputDirectory =
                resolvedConfig.OutputDirectory ?? temporaryDirectory.CreateSubdirectory("out");

            var baselineRun = BenchmarkRun.FromSourcePath(
                sourceDirectory: baselineDirectory,
                outputDirectory: outputDirectory.CreateSubdirectory("baseline"),
                name: "baseline",
                benchmarks: resolvedConfig.Benchmarks
            );

            var targetRun = BenchmarkRun.FromSourcePath(
                sourceDirectory: targetDirectory,
                outputDirectory: outputDirectory.CreateSubdirectory("target"),
                name: "target",
                benchmarks: resolvedConfig.Benchmarks
            );

            var results = BenchmarkComparer.CompareBenchmarks(
                baselineRun,
                targetRun,
                resolvedConfig.Verbose
            );

            var reporter = Reporting.CreateReporter(
                resolvedConfig.OutputStyle,
                outputDirectory,
                Console.Out,
                useColors: false,
                isInteractiveMode: false
            );
            reporter.GenerateReport(results);
        }
        catch (Exception ex)
        {
            Output.Error(ex.Message);
            Environment.Exit(1);
        }
    }
}
