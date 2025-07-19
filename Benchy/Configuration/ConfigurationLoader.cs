using Benchy.Infrastructure;
using Tomlyn;

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
        return ResolveConfiguration(fileConfig, argsConfig, mode);
    }

    private static ConfigFromFile? LoadConfigurationFromFile(DirectoryInfo basePath)
    {
        var filePath = FindConfigurationFile(basePath);
        if (filePath == null || !filePath.Exists)
        {
            return null;
        }

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
        Mode mode
    )
    {
        var globalConfig = fileConfig;
        var modeConfig = mode switch
        {
            Mode.Interactive => fileConfig?.Interactive,
            Mode.Ci => fileConfig?.Ci,
        };

        var verbose = argsConfig.Verbose ?? modeConfig?.Verbose ?? globalConfig?.Verbose ?? false;
        var outputDirectory =
            argsConfig.OutputDirectory
            ?? modeConfig?.OutputDirectory
            ?? globalConfig?.OutputDirectory;
        var outputStyle =
            argsConfig.OutputStyle ?? modeConfig?.OutputStyle ?? globalConfig?.OutputStyle ?? [];
        var benchmarks =
            argsConfig.Benchmarks ?? modeConfig?.Benchmarks ?? globalConfig?.Benchmarks ?? [];
        var noDelete =
            argsConfig.NoDelete ?? modeConfig?.NoDelete ?? globalConfig?.NoDelete ?? false;

        return new ResolvedConfig
        {
            Verbose = verbose,
            OutputDirectory = outputDirectory != null ? new DirectoryInfo(outputDirectory) : null,
            OutputStyle = outputStyle,
            Benchmarks = benchmarks,
            NoDelete = noDelete,
        };
    }

    private static FileInfo? FindConfigurationFile(DirectoryInfo basePath)
    {
        return TryFile(basePath, ".config/benchy.toml")
            ?? TryFile(basePath, "benchy.toml")
            ?? TryFile(basePath, ".benchy.toml");

        static FileInfo? TryFile(DirectoryInfo dir, string fileName)
        {
            var fullPath = Path.Combine(dir.FullName, fileName);
            return File.Exists(fullPath) ? new FileInfo(fullPath) : null;
        }
    }

    public enum Mode
    {
        Interactive,
        Ci,
    }
}
