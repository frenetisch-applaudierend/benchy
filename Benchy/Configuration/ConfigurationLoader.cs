using Tomlyn;

namespace Benchy.Configuration;

public static class ConfigurationLoader
{
    private static readonly string[] ConfigFileNames =
    [
        ".config/benchy.toml",
        "benchy.toml",
        ".benchy.toml",
    ];

    public static BenchyConfig LoadConfiguration()
    {
        var filePath = FindConfigurationFile();

        if (filePath == null || !File.Exists(filePath))
        {
            return new BenchyConfig(); // Return empty config with defaults
        }

        try
        {
            var tomlContent = File.ReadAllText(filePath);
            var config = Toml.ToModel<BenchyConfig>(tomlContent);
            return config ?? new BenchyConfig();
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException(
                $"Failed to parse configuration file '{filePath}': {ex.Message}",
                ex
            );
        }
    }

    public static ResolvedConfig ResolveConfiguration(
        BenchyConfig fileConfig,
        bool isInteractiveMode,
        // Command-line overrides
        bool? verboseOverride = null,
        DirectoryInfo? outputDirectoryOverride = null,
        string[]? outputStyleOverride = null,
        string[]? benchmarksOverride = null,
        bool? noDeleteOverride = null
    )
    {
        var resolved = ResolvedConfig.CreateDefault(isInteractiveMode);

        // Apply configuration layers in precedence order:
        // 1. Defaults (already applied)
        // 2. Global config
        // 3. Command-specific config (interactive/ci)
        // 4. Command-line overrides

        ApplyGlobalConfig(resolved, fileConfig.Global);

        if (isInteractiveMode)
        {
            ApplyInteractiveConfig(resolved, fileConfig.Interactive);
        }
        else
        {
            ApplyCiConfig(resolved, fileConfig.Ci);
        }

        // Apply command-line overrides
        ApplyCommandLineOverrides(
            resolved,
            verboseOverride,
            outputDirectoryOverride,
            outputStyleOverride,
            benchmarksOverride,
            noDeleteOverride
        );

        return resolved;
    }

    private static string? FindConfigurationFile()
    {
        var currentDirectory = Directory.GetCurrentDirectory();

        foreach (var fileName in ConfigFileNames)
        {
            var fullPath = Path.Combine(currentDirectory, fileName);
            if (File.Exists(fullPath))
            {
                return fullPath;
            }
        }

        return null;
    }

    private static void ApplyGlobalConfig(ResolvedConfig resolved, GlobalConfig global)
    {
        if (global.Verbose.HasValue)
            resolved.Verbose = global.Verbose.Value;
        if (!string.IsNullOrEmpty(global.OutputDirectory))
            resolved.OutputDirectory = new DirectoryInfo(global.OutputDirectory);
        if (global.OutputStyle?.Length > 0)
            resolved.OutputStyle = global.OutputStyle;
        if (global.Benchmarks?.Length > 0)
            resolved.Benchmarks = global.Benchmarks;
    }

    private static void ApplyInteractiveConfig(
        ResolvedConfig resolved,
        InteractiveConfig interactive
    )
    {
        if (interactive.Verbose.HasValue)
            resolved.Verbose = interactive.Verbose.Value;
        if (!string.IsNullOrEmpty(interactive.OutputDirectory))
            resolved.OutputDirectory = new DirectoryInfo(interactive.OutputDirectory);
        if (interactive.OutputStyle?.Length > 0)
            resolved.OutputStyle = interactive.OutputStyle;
        if (interactive.Benchmarks?.Length > 0)
            resolved.Benchmarks = interactive.Benchmarks;
        if (interactive.NoDelete.HasValue)
            resolved.NoDelete = interactive.NoDelete.Value;
    }

    private static void ApplyCiConfig(ResolvedConfig resolved, CiConfig ci)
    {
        if (ci.Verbose.HasValue)
            resolved.Verbose = ci.Verbose.Value;
        if (!string.IsNullOrEmpty(ci.OutputDirectory))
            resolved.OutputDirectory = new DirectoryInfo(ci.OutputDirectory);
        if (ci.OutputStyle?.Length > 0)
            resolved.OutputStyle = ci.OutputStyle;
        if (ci.Benchmarks?.Length > 0)
            resolved.Benchmarks = ci.Benchmarks;
    }

    private static void ApplyCommandLineOverrides(
        ResolvedConfig resolved,
        bool? verboseOverride,
        DirectoryInfo? outputDirectoryOverride,
        string[]? outputStyleOverride,
        string[]? benchmarksOverride,
        bool? noDeleteOverride
    )
    {
        if (verboseOverride.HasValue)
            resolved.Verbose = verboseOverride.Value;
        if (outputDirectoryOverride != null)
            resolved.OutputDirectory = outputDirectoryOverride;
        if (outputStyleOverride?.Length > 0)
            resolved.OutputStyle = outputStyleOverride;
        if (benchmarksOverride?.Length > 0)
            resolved.Benchmarks = benchmarksOverride;
        if (noDeleteOverride.HasValue)
            resolved.NoDelete = noDeleteOverride.Value;
    }
}
