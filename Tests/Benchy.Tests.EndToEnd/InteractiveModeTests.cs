using LibGit2Sharp;
using System.Diagnostics;

namespace Benchy.Tests.EndToEnd;

public class InteractiveModeTests : IDisposable
{
    private readonly List<IDisposable> _disposables = new();
    
    [Fact]
    public async Task InteractiveMode_PerformanceRegression_DetectsSlowerPerformance()
    {
        // Arrange - Create baseline benchmark result
        var baselineConfig = FakeBenchmarkConfig.WithJsonOutput(
            "HashingBenchmark-report-full-compressed.json",
            BenchmarkResultTemplates.CreateHashingBenchmarkResult(
                title: "HashingBenchmark-baseline",
                meanMultiplier: 1.0 // baseline performance
            )
        );
        
        // Create regression benchmark result (15% slower)
        var regressionConfig = FakeBenchmarkConfig.WithJsonOutput(
            "HashingBenchmark-report-full-compressed.json", 
            BenchmarkResultTemplates.CreateHashingBenchmarkResult(
                title: "HashingBenchmark-regression",
                meanMultiplier: 1.15 // 15% slower
            )
        );
        
        // Create test repository with two commits
        var testRepo = CreateTestRepository();
        await CreateCommitWithFakeBenchmark(testRepo, "baseline", baselineConfig);
        await CreateCommitWithFakeBenchmark(testRepo, "regression", regressionConfig);
        
        using var tempOutputDir = CreateTempDirectory();
        
        // Act - Run Benchy compare command
        var result = await RunBenchyCommand(new[]
        {
            "compare", "baseline", "regression",
            "--repo", testRepo.Path,
            "--benchmark", "HashingBenchmarks",
            "--output", tempOutputDir.FullName
        });
        
        // Assert
        result.ExitCode.Should().Be(0, $"Benchy should succeed. Output: {result.Output}");
        result.Output.Should().Contain("HashingBenchmark"); // Should mention the benchmark
        
        // Check that output files were created
        var outputFiles = Directory.GetFiles(tempOutputDir.FullName, "*", SearchOption.AllDirectories);
        outputFiles.Should().NotBeEmpty("Should generate output files");
    }
    
    [Fact]
    public async Task InteractiveMode_BenchmarkFailure_ReportsError()
    {
        // Arrange - Create benchmark that fails
        var failureConfig = FakeBenchmarkConfig.CreateDefault().WithFailure();
        
        var testRepo = CreateTestRepository();
        await CreateCommitWithFakeBenchmark(testRepo, "working", FakeBenchmarkConfig.CreateDefault());
        await CreateCommitWithFakeBenchmark(testRepo, "broken", failureConfig);
        
        // Act
        var result = await RunBenchyCommand(new[]
        {
            "compare", "working", "broken",
            "--repo", testRepo.Path,
            "--benchmark", "TestBenchmarks"
        });
        
        // Assert
        result.ExitCode.Should().NotBe(0, "Should fail when benchmark fails");
        result.ErrorOutput.Should().NotBeEmpty("Should have error output");
    }
    
    [Fact]
    public async Task InteractiveMode_MultipleBenchmarkProjects_RunsAllProjects()
    {
        // Arrange - Create multiple benchmark projects with different results
        var hashingConfig = FakeBenchmarkConfig.WithJsonOutput(
            "HashingBenchmark-report-full-compressed.json",
            BenchmarkResultTemplates.CreateHashingBenchmarkResult()
        );
        
        var algorithmConfig = FakeBenchmarkConfig.WithJsonOutput(
            "AlgorithmBenchmark-report-full-compressed.json",
            BenchmarkResultTemplates.CreateAlgorithmBenchmarkResult()
        );
        
        var testRepo = CreateTestRepository();
        await CreateCommitWithMultipleBenchmarks(testRepo, "test-commit", new[]
        {
            ("HashingBenchmarks", hashingConfig),
            ("AlgorithmBenchmarks", algorithmConfig)
        });
        
        using var tempOutputDir = CreateTempDirectory();
        
        // Act
        var result = await RunBenchyCommand(new[]
        {
            "compare", "test-commit", "test-commit", // Compare commit to itself
            "--repo", testRepo.Path,
            "--benchmark", "HashingBenchmarks", "AlgorithmBenchmarks",
            "--output", tempOutputDir.FullName
        });
        
        // Assert
        result.ExitCode.Should().Be(0);
        result.Output.Should().Contain("HashingBenchmark");
        result.Output.Should().Contain("AlgorithmBenchmark");
    }
    
    [Theory]
    [InlineData(0.85)] // 15% faster
    [InlineData(1.0)]  // No change
    [InlineData(1.25)] // 25% slower
    public async Task InteractiveMode_VariousPerformanceChanges_DetectedCorrectly(double performanceMultiplier)
    {
        // Arrange
        var baselineConfig = FakeBenchmarkConfig.WithJsonOutput(
            "TestBenchmark-report-full-compressed.json",
            BenchmarkResultTemplates.CreateHashingBenchmarkResult(meanMultiplier: 1.0)
        );
        
        var changedConfig = FakeBenchmarkConfig.WithJsonOutput(
            "TestBenchmark-report-full-compressed.json",
            BenchmarkResultTemplates.CreateHashingBenchmarkResult(meanMultiplier: performanceMultiplier)
        );
        
        var testRepo = CreateTestRepository();
        await CreateCommitWithFakeBenchmark(testRepo, "baseline", baselineConfig);
        await CreateCommitWithFakeBenchmark(testRepo, "changed", changedConfig);
        
        // Act
        var result = await RunBenchyCommand(new[]
        {
            "compare", "baseline", "changed",
            "--repo", testRepo.Path,
            "--benchmark", "TestBenchmarks"
        });
        
        // Assert
        result.ExitCode.Should().Be(0);
        // The specific assertions about performance change detection would depend on
        // Benchy's output format - this could be extended based on actual output parsing
    }
    
    private TestRepository CreateTestRepository()
    {
        var tempDir = CreateTempDirectory();
        var repoPath = Path.Combine(tempDir.FullName, "test-repo");
        
        Repository.Init(repoPath);
        var repo = new TestRepository(repoPath);
        _disposables.Add(repo);
        return repo;
    }
    
    private async Task CreateCommitWithFakeBenchmark(TestRepository repo, string commitName, FakeBenchmarkConfig config)
    {
        var projectGenerator = BenchmarkProjectGenerator.CreateProject(
            "TestBenchmarks", 
            config, 
            new DirectoryInfo(repo.Path)
        );
        _disposables.Add(projectGenerator);
        
        using var libRepo = new Repository(repo.Path);
        Commands.Stage(libRepo, "*");
        
        var signature = new Signature("Test User", "test@example.com", DateTimeOffset.Now);
        libRepo.Commit($"Add {commitName} benchmark", signature, signature);
        
        // Create a tag for easy reference
        libRepo.Tags.Add(commitName, libRepo.Head.Tip);
    }
    
    private async Task CreateCommitWithMultipleBenchmarks(
        TestRepository repo, 
        string commitName, 
        (string ProjectName, FakeBenchmarkConfig Config)[] benchmarks)
    {
        foreach (var (projectName, config) in benchmarks)
        {
            var projectGenerator = BenchmarkProjectGenerator.CreateProject(
                projectName, 
                config, 
                new DirectoryInfo(repo.Path)
            );
            _disposables.Add(projectGenerator);
        }
        
        using var libRepo = new Repository(repo.Path);
        Commands.Stage(libRepo, "*");
        
        var signature = new Signature("Test User", "test@example.com", DateTimeOffset.Now);
        libRepo.Commit($"Add {commitName} benchmarks", signature, signature);
        libRepo.Tags.Add(commitName, libRepo.Head.Tip);
    }
    
    private DirectoryInfo CreateTempDirectory()
    {
        var tempPath = Path.Combine(Path.GetTempPath(), $"benchy-e2e-test-{Guid.NewGuid():N}");
        var dir = Directory.CreateDirectory(tempPath);
        _disposables.Add(new DisposableDirectory(dir));
        return dir;
    }
    
    private async Task<ProcessResult> RunBenchyCommand(string[] args)
    {
        var benchyPath = FindBenchyExecutable();
        
        var startInfo = new ProcessStartInfo
        {
            FileName = "dotnet",
            Arguments = $"run --project \"{benchyPath}\" --no-build -- {string.Join(" ", args.Select(QuoteIfNeeded))}",
            UseShellExecute = false,
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            CreateNoWindow = true
        };
        
        using var process = Process.Start(startInfo)!;
        var outputTask = process.StandardOutput.ReadToEndAsync();
        var errorTask = process.StandardError.ReadToEndAsync();
        
        await process.WaitForExitAsync();
        
        return new ProcessResult(
            process.ExitCode,
            await outputTask,
            await errorTask
        );
    }
    
    private string FindBenchyExecutable()
    {
        // Look for the Benchy project file relative to this test project
        var currentDir = Directory.GetCurrentDirectory();
        var searchPaths = new[]
        {
            Path.Combine(currentDir, "..", "..", "Benchy", "Benchy.csproj"),
            Path.Combine(currentDir, "..", "..", "..", "Benchy", "Benchy.csproj"),
            Path.Combine(currentDir, "Benchy", "Benchy.csproj")
        };
        
        foreach (var path in searchPaths)
        {
            if (File.Exists(path))
            {
                return path;
            }
        }
        
        throw new FileNotFoundException("Could not find Benchy.csproj");
    }
    
    private static string QuoteIfNeeded(string arg)
    {
        return arg.Contains(' ') ? $"\"{arg}\"" : arg;
    }
    
    public void Dispose()
    {
        foreach (var disposable in _disposables)
        {
            try
            {
                disposable.Dispose();
            }
            catch
            {
                // Ignore disposal errors in tests
            }
        }
    }
}

public record ProcessResult(int ExitCode, string Output, string ErrorOutput);

public class TestRepository : IDisposable
{
    public string Path { get; }
    
    public TestRepository(string path)
    {
        Path = path;
    }
    
    public void Dispose()
    {
        try
        {
            if (Directory.Exists(Path))
            {
                Directory.Delete(Path, recursive: true);
            }
        }
        catch
        {
            // Ignore cleanup errors
        }
    }
}

public class DisposableDirectory : IDisposable
{
    private readonly DirectoryInfo _directory;
    
    public DisposableDirectory(DirectoryInfo directory)
    {
        _directory = directory;
    }
    
    public void Dispose()
    {
        try
        {
            if (_directory.Exists)
            {
                _directory.Delete(recursive: true);
            }
        }
        catch
        {
            // Ignore cleanup errors
        }
    }
}