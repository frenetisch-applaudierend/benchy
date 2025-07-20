using Benchy.Configuration;
using Benchy.Infrastructure;

namespace Benchy.Tests.Unit.Configuration;

public class ConfigurationLoaderTests
{
    [Fact]
    public void LoadConfiguration_NoConfigFile_UsesDefaults()
    {
        // Arrange
        using var tempDir = TemporaryDirectory.CreateNew();
        var basePath = tempDir.Directory;
        var argsConfig = new ConfigFromArgs();

        // Act
        var result = ConfigurationLoader.LoadConfiguration(
            basePath,
            argsConfig,
            tempDir,
            ConfigurationLoader.Mode.Interactive
        );

        // Assert
        result.Should().NotBeNull();
        result.Verbose.Should().BeFalse();
        result.NoDelete.Should().BeFalse();
        result.OutputStyle.Should().Equal("console");
        result.Benchmarks.Should().BeEmpty();
    }

    [Fact]
    public void LoadConfiguration_InteractiveMode_UsesInteractiveDefaults()
    {
        // Arrange
        using var tempDir = TemporaryDirectory.CreateNew();
        var basePath = tempDir.Directory;
        var argsConfig = new ConfigFromArgs();

        // Act
        var result = ConfigurationLoader.LoadConfiguration(
            basePath,
            argsConfig,
            tempDir,
            ConfigurationLoader.Mode.Interactive
        );

        // Assert
        result.Verbose.Should().BeFalse();
        result.NoDelete.Should().BeFalse();
        result.OutputStyle.Should().Equal("console");
    }

    [Fact]
    public void LoadConfiguration_CiMode_UsesCiDefaults()
    {
        // Arrange
        using var tempDir = TemporaryDirectory.CreateNew();
        var basePath = tempDir.Directory;
        var argsConfig = new ConfigFromArgs();

        // Act
        var result = ConfigurationLoader.LoadConfiguration(
            basePath,
            argsConfig,
            tempDir,
            ConfigurationLoader.Mode.Ci
        );

        // Assert
        result.Verbose.Should().BeFalse();
        result.NoDelete.Should().BeTrue();
        result.OutputStyle.Should().Equal("json", "markdown");
    }

    [Fact]
    public void LoadConfiguration_ArgsOverrideDefaults_UsesArgsValues()
    {
        // Arrange
        using var tempDir = TemporaryDirectory.CreateNew();
        var basePath = tempDir.Directory;
        var argsConfig = new ConfigFromArgs
        {
            Verbose = true,
            NoDelete = true,
            OutputStyle = ["custom"],
            Benchmarks = ["TestBenchmark"],
            OutputDirectory = "/custom/output",
        };

        // Act
        var result = ConfigurationLoader.LoadConfiguration(
            basePath,
            argsConfig,
            tempDir,
            ConfigurationLoader.Mode.Interactive
        );

        // Assert
        result.Verbose.Should().BeTrue();
        result.NoDelete.Should().BeTrue();
        result.OutputStyle.Should().Equal("custom");
        result.Benchmarks.Should().Equal("TestBenchmark");
        result.OutputDirectory.FullName.Should().Be("/custom/output");
    }

    [Fact]
    public void LoadConfiguration_NoOutputDirectory_UsesTemporarySubdirectory()
    {
        // Arrange
        using var tempDir = TemporaryDirectory.CreateNew();
        var basePath = tempDir.Directory;
        var argsConfig = new ConfigFromArgs();

        // Act
        var result = ConfigurationLoader.LoadConfiguration(
            basePath,
            argsConfig,
            tempDir,
            ConfigurationLoader.Mode.Interactive
        );

        // Assert
        result.OutputDirectory.FullName.Should().Be(Path.Combine(tempDir.FullName, "out"));
    }

    [Fact]
    public void LoadConfiguration_WithConfigFile_LoadsFromFile()
    {
        // Arrange
        using var tempDir = TemporaryDirectory.CreateNew();
        var basePath = tempDir.Directory;

        var configContent = """
            verbose = true
            benchmarks = ["FileBenchmark"]

            [interactive]
            output_style = ["console", "json"]

            [ci]
            no_delete = false
            """;

        var configFile = Path.Combine(basePath.FullName, "benchy.toml");
        File.WriteAllText(configFile, configContent);

        var argsConfig = new ConfigFromArgs();

        // Act
        var result = ConfigurationLoader.LoadConfiguration(
            basePath,
            argsConfig,
            tempDir,
            ConfigurationLoader.Mode.Interactive
        );

        // Assert
        result.Verbose.Should().BeTrue();
        result.Benchmarks.Should().Equal("FileBenchmark");
        result.OutputStyle.Should().Equal("console", "json");
    }

    [Theory]
    [InlineData(".config/benchy.toml")]
    [InlineData("benchy.toml")]
    [InlineData(".benchy.toml")]
    public void LoadConfiguration_FindsConfigFile_InExpectedLocations(string configPath)
    {
        // Arrange
        using var tempDir = TemporaryDirectory.CreateNew();
        var basePath = tempDir.Directory;

        var configContent = """
            verbose = true
            benchmarks = ["TestBenchmark"]
            """;

        var fullConfigPath = Path.Combine(basePath.FullName, configPath);
        Directory.CreateDirectory(Path.GetDirectoryName(fullConfigPath)!);
        File.WriteAllText(fullConfigPath, configContent);

        var argsConfig = new ConfigFromArgs();

        // Act
        var result = ConfigurationLoader.LoadConfiguration(
            basePath,
            argsConfig,
            tempDir,
            ConfigurationLoader.Mode.Interactive
        );

        // Assert
        result.Verbose.Should().BeTrue();
        result.Benchmarks.Should().Equal("TestBenchmark");
    }

    [Fact]
    public void LoadConfiguration_InvalidConfigFile_ThrowsInvalidOperationException()
    {
        // Arrange
        using var tempDir = TemporaryDirectory.CreateNew();
        var basePath = tempDir.Directory;

        var invalidConfigContent = """
            verbose = true
            invalid syntax here [
            """;

        var configFile = Path.Combine(basePath.FullName, "benchy.toml");
        File.WriteAllText(configFile, invalidConfigContent);

        var argsConfig = new ConfigFromArgs();

        // Act & Assert
        var act = () =>
            ConfigurationLoader.LoadConfiguration(
                basePath,
                argsConfig,
                tempDir,
                ConfigurationLoader.Mode.Interactive
            );

        act.Should()
            .Throw<InvalidOperationException>()
            .WithMessage("Failed to parse configuration file*");
    }

    [Fact]
    public void LoadConfiguration_ConfigPrecedence_ArgsOverrideModeOverrideGlobal()
    {
        // Arrange
        using var tempDir = TemporaryDirectory.CreateNew();
        var basePath = tempDir.Directory;

        var configContent = """
            verbose = false
            output_style = ["global"]

            [interactive]
            verbose = true
            output_style = ["interactive"]
            """;

        var configFile = Path.Combine(basePath.FullName, "benchy.toml");
        File.WriteAllText(configFile, configContent);

        var argsConfig = new ConfigFromArgs { OutputStyle = ["args"] };

        // Act
        var result = ConfigurationLoader.LoadConfiguration(
            basePath,
            argsConfig,
            tempDir,
            ConfigurationLoader.Mode.Interactive
        );

        // Assert
        result.Verbose.Should().BeTrue(); // From interactive config (overrides global)
        result.OutputStyle.Should().Equal("args"); // From args (overrides interactive and global)
    }

    [Fact]
    public void LoadConfiguration_EmptyOutputStyleArrays_FallsBackToDefaults()
    {
        // Arrange
        using var tempDir = TemporaryDirectory.CreateNew();
        var basePath = tempDir.Directory;

        var configContent = """
            output_style = []

            [interactive]
            output_style = []
            """;

        var configFile = Path.Combine(basePath.FullName, "benchy.toml");
        File.WriteAllText(configFile, configContent);

        var argsConfig = new ConfigFromArgs { OutputStyle = [] };

        // Act
        var result = ConfigurationLoader.LoadConfiguration(
            basePath,
            argsConfig,
            tempDir,
            ConfigurationLoader.Mode.Interactive
        );

        // Assert
        result.OutputStyle.Should().Equal("console"); // Falls back to interactive mode defaults
    }

    [Fact]
    public void LoadConfiguration_TemporaryDirectorySet_UsesProvidedDirectory()
    {
        // Arrange
        using var tempDir = TemporaryDirectory.CreateNew();
        var basePath = tempDir.Directory;
        var argsConfig = new ConfigFromArgs();

        // Act
        var result = ConfigurationLoader.LoadConfiguration(
            basePath,
            argsConfig,
            tempDir,
            ConfigurationLoader.Mode.Interactive
        );

        // Assert
        result.TemporaryDirectory.FullName.Should().Be(tempDir.Directory.FullName);
    }
}
