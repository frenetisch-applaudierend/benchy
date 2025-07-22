using Benchy.Infrastructure;
using Benchy.Output;
using Tomlyn;
using static Benchy.Output.FormattedText;

namespace Benchy.Configuration;

public static class ConfigurationLoader
{
    public static ResolvedConfig LoadConfiguration(
        DirectoryInfo basePath,
        ConfigFromArgs argsConfig,
        TemporaryDirectory temporaryDirectory,
        Mode mode
    )
    {
        var fileConfig = LoadConfigurationFromFile(basePath);
        return ResolveConfiguration(fileConfig, argsConfig, temporaryDirectory, mode);
    }

    private static ConfigFromFile? LoadConfigurationFromFile(DirectoryInfo basePath)
    {
        var filePath = FindConfigurationFile(basePath);
        if (filePath == null || !filePath.Exists)
        {
            CliOutput.Verbose(
                $"No configuration file found in '{Em(basePath.FullName)}'. Using defaults."
            );
            return null;
        }

        CliOutput.Verbose($"Found configuration file at '{Em(filePath.FullName)}'");

        try
        {
            var tomlContent = File.ReadAllText(filePath.FullName);
            return Toml.ToModel<ConfigFromFile>(tomlContent);
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException(
                $"Failed to parse configuration file '{filePath.FullName}': {ex.Message}",
                ex
            );
        }
    }

    private static ResolvedConfig ResolveConfiguration(
        ConfigFromFile? fileConfig,
        ConfigFromArgs argsConfig,
        TemporaryDirectory temporaryDirectory,
        Mode mode
    )
    {
        var globalConfig = fileConfig;
        var modeConfig = mode switch
        {
            Mode.Interactive => fileConfig?.Interactive,
            Mode.Ci => fileConfig?.Ci,
        };
        var defaults = GetDefaults(mode);

        var verbose =
            argsConfig.Verbose ?? modeConfig?.Verbose ?? globalConfig?.Verbose ?? defaults.Verbose;
        var outputDirectory =
            argsConfig.OutputDirectory
            ?? modeConfig?.OutputDirectory
            ?? globalConfig?.OutputDirectory;
        var outputStyle = ResolveArrayValue(
            argsConfig.OutputStyle,
            modeConfig?.OutputStyle,
            globalConfig?.OutputStyle,
            defaults.OutputStyle
        );
        var benchmarks = ResolveArrayValue(
            argsConfig.Benchmarks,
            modeConfig?.Benchmarks,
            globalConfig?.Benchmarks,
            []
        );
        var noDelete =
            argsConfig.NoDelete
            ?? modeConfig?.NoDelete
            ?? globalConfig?.NoDelete
            ?? defaults.NoDelete;
        var significanceThreshold =
            argsConfig.SignificanceThreshold
            ?? modeConfig?.SignificanceThreshold
            ?? globalConfig?.SignificanceThreshold
            ?? defaults.SignificanceThreshold;
        var decoration = ResolveDecoration(
            argsConfig.Decoration,
            modeConfig?.Decoration,
            globalConfig?.Decoration,
            mode
        );

        return new ResolvedConfig
        {
            TemporaryDirectory = temporaryDirectory.Directory,
            Verbose = verbose,
            OutputDirectory =
                outputDirectory != null
                    ? new DirectoryInfo(outputDirectory)
                    : temporaryDirectory.CreateSubdirectory("out"),
            OutputStyle = outputStyle,
            Benchmarks = benchmarks,
            NoDelete = noDelete,
            SignificanceThreshold = significanceThreshold,
            Decoration = decoration,
        };

        static string[] ResolveArrayValue(
            string[]? argsValue,
            string[]? modeValue,
            string[]? globalValue,
            string[] defaults
        )
        {
            if (argsValue != null && argsValue.Length > 0)
            {
                return argsValue;
            }

            if (modeValue != null && modeValue.Length > 0)
            {
                return modeValue;
            }

            if (globalValue != null && globalValue.Length > 0)
            {
                return globalValue;
            }

            return defaults;
        }

        static bool ResolveDecoration(
            bool? argsValue,
            bool? modeValue,
            bool? globalValue,
            Mode mode
        )
        {
            if (argsValue.HasValue)
            {
                return argsValue.Value;
            }

            if (modeValue.HasValue)
            {
                return modeValue.Value;
            }

            if (globalValue.HasValue)
            {
                return globalValue.Value;
            }

            // Smart defaults based on mode and output detection
            return mode switch
            {
                Mode.Ci => false,
                Mode.Interactive => !IsOutputRedirected(),
                _ => false,
            };
        }
    }

    private static bool IsOutputRedirected()
    {
        // Check if stdout is redirected (piped or redirected to file)
        // In .NET, Console.IsOutputRedirected returns true when output is piped or redirected
        return Console.IsOutputRedirected;
    }

    private static FileInfo? FindConfigurationFile(DirectoryInfo basePath)
    {
        return TryFile(basePath, ".config/benchy.toml")
            ?? TryFile(basePath, "benchy.toml")
            ?? TryFile(basePath, ".benchy.toml");

        static FileInfo? TryFile(DirectoryInfo dir, string fileName)
        {
            var fullPath = Path.Combine(dir.FullName, fileName);
            CliOutput.Verbose($"Checking for configuration file at '{Em(fullPath)}'");
            return File.Exists(fullPath) ? new FileInfo(fullPath) : null;
        }
    }

    private static Defaults GetDefaults(Mode mode)
    {
        return mode switch
        {
            Mode.Interactive => new Defaults
            {
                Verbose = false,
                NoDelete = false,
                OutputStyle = ["console"],
                SignificanceThreshold = 0.05, // 5%
            },
            Mode.Ci => new Defaults
            {
                Verbose = false,
                NoDelete = true,
                OutputStyle = ["json", "markdown"],
                SignificanceThreshold = 0.05, // 5%
            },
        };
    }

    public enum Mode
    {
        Interactive,
        Ci,
    }

    private class Defaults
    {
        public required bool Verbose { get; init; }
        public required bool NoDelete { get; init; }
        public required string[] OutputStyle { get; init; }
        public required double SignificanceThreshold { get; init; }
    }
}
