# Benchy Testing Strategy

## Overview

This document outlines a comprehensive testing strategy for the Benchy CLI tool that combines unit tests, integration tests, and end-to-end tests while addressing the key challenge of slow real benchmark execution.

## Testing Challenges

1. **Performance**: Real BenchmarkDotNet executions are slow (minutes per benchmark)
2. **Dependencies**: Git operations, file system, .NET builds, and external processes
3. **Isolation**: Tests must not interfere with each other or the host system
4. **Realism**: Tests should validate actual CLI behavior while remaining fast

## Testing Architecture

### 1. Unit Tests

**Scope**: Test individual classes and methods in isolation

**Target Components**:
- `BenchmarkReport` JSON parsing and data models
- `DirectoryInfoExtensions` utility methods
- `Arguments` CLI parameter validation
- Output formatting logic
- Configuration loading and validation

**Implementation**:
```csharp
// Example: BenchmarkReport parsing
[Test]
public void BenchmarkReport_ParseValidJson_ReturnsCorrectStructure()
{
    var json = CreateSampleBenchmarkJson();
    var report = BenchmarkReport.FromJson(json);
    
    Assert.That(report.Title, Is.EqualTo("MyBenchmark"));
    Assert.That(report.Benchmarks, Has.Count.EqualTo(2));
}
```

**Tools**: 
- NUnit or xUnit for test framework
- Moq for mocking dependencies
- FluentAssertions for readable assertions

### 2. Integration Tests

**Scope**: Test component interactions with controlled dependencies

#### 2.1 Git Operations Testing
- Use lightweight test repositories in `Tests/TestData/`
- Create minimal Git repos with known commit structure
- Test checkout, cloning, and reference resolution

```csharp
[Test]
public void GitRepository_CheckoutCommit_CreatesCorrectWorkingDirectory()
{
    using var testRepo = TestGitRepository.Create();
    testRepo.AddCommit("initial", new[] { ("file.txt", "content1") });
    testRepo.AddCommit("second", new[] { ("file.txt", "content2") });
    
    var git = new GitRepository(testRepo.Path);
    git.CheckoutCommit("initial");
    
    Assert.That(File.ReadAllText("file.txt"), Is.EqualTo("content1"));
}
```

#### 2.2 Project Building with Test Doubles
- Create minimal test projects that build quickly
- Mock `DotnetProject` operations for speed
- Verify build commands without actual execution

```csharp
[Test]
public void DotnetProject_Build_InvokesCorrectCommand()
{
    var mockProcessRunner = new Mock&lt;IProcessRunner&gt;();
    var project = new DotnetProject(mockProcessRunner.Object);
    
    project.Build("TestProject.csproj");
    
    mockProcessRunner.Verify(x => x.Run(
        "dotnet", 
        "build \"TestProject.csproj\" --configuration Release",
        It.IsAny&lt;string&gt;()));
}
```

### 3. End-to-End Tests

**Scope**: Test complete CLI workflows with fake benchmarks

#### 3.1 BenchmarkDotNet Fake Implementation

**Concept**: Replace slow BenchmarkDotNet execution with fast fake benchmark projects that mimic the exact BenchmarkDotNet CLI interface and output format.

**Key Design Principles**:
1. **Identical CLI Interface**: Accept all BenchmarkDotNet arguments exactly as real benchmarks do
2. **Authentic Output Format**: Generate JSON that matches the real BenchmarkDotNet output structure  
3. **Configurable Results**: Support different performance scenarios via environment variables
4. **Fast Execution**: Complete in milliseconds instead of minutes

**Implementation Architecture**:

##### Core Fake Benchmark Project
```csharp
// Tests/TestUtilities/FakeBenchmark/Program.cs
using System;
using System.CommandLine;
using System.IO;
using System.Text.Json;

public static class Program
{
    public static async Task<int> Main(string[] args)
    {
        var parser = CreateBenchmarkDotNetCommandLine();
        return await parser.InvokeAsync(args);
    }
    
    private static Command CreateBenchmarkDotNetCommandLine()
    {
        var rootCommand = new RootCommand("Fake BenchmarkDotNet Runner");
        
        // Add all BenchmarkDotNet options that Benchy uses
        rootCommand.AddOption(new Option<bool>("--keepFiles"));
        rootCommand.AddOption(new Option<bool>("--stopOnFirstError"));  
        rootCommand.AddOption(new Option<bool>("--memory"));
        rootCommand.AddOption(new Option<bool>("--threading"));
        rootCommand.AddOption(new Option<string[]>("--exporters"));
        rootCommand.AddOption(new Option<DirectoryInfo>("--artifacts"));
        
        rootCommand.SetHandler(ExecuteFakeBenchmark);
        return rootCommand;
    }
    
    private static void ExecuteFakeBenchmark(
        bool keepFiles,
        bool stopOnFirstError, 
        bool memory,
        bool threading,
        string[] exporters,
        DirectoryInfo artifacts)
    {
        var config = BenchmarkConfiguration.FromEnvironment();
        var generator = new BenchmarkResultGenerator(config);
        
        // Generate results based on the current directory/commit context
        var results = generator.GenerateResults();
        
        // Write to expected BenchmarkDotNet output location
        var outputFile = Path.Combine(
            artifacts?.FullName ?? "BenchmarkDotNet.Artifacts",
            $"{results.Title}-report-full-compressed.json"
        );
        
        Directory.CreateDirectory(Path.GetDirectoryName(outputFile)!);
        File.WriteAllText(outputFile, JsonSerializer.Serialize(results, JsonOptions));
        
        Console.WriteLine($"// Generated fake benchmark results in {outputFile}");
    }
}
```

##### BenchmarkConfiguration and Result Generation
```csharp
// Tests/TestUtilities/FakeBenchmark/BenchmarkConfiguration.cs
public class BenchmarkConfiguration
{
    public string Scenario { get; set; } = "baseline";
    public string BenchmarkName { get; set; } = "HashingBenchmark";
    public Dictionary<string, double> PerformanceMultipliers { get; set; } = new();
    
    public static BenchmarkConfiguration FromEnvironment()
    {
        return new BenchmarkConfiguration
        {
            Scenario = Environment.GetEnvironmentVariable("FAKE_BENCHMARK_SCENARIO") ?? "baseline",
            BenchmarkName = Environment.GetEnvironmentVariable("FAKE_BENCHMARK_NAME") ?? "HashingBenchmark",
            PerformanceMultipliers = ParseMultipliers(
                Environment.GetEnvironmentVariable("FAKE_BENCHMARK_MULTIPLIERS") ?? "")
        };
    }
    
    private static Dictionary<string, double> ParseMultipliers(string multipliers)
    {
        // Parse format: "MD5=1.0,SHA256=1.2" for 20% slower SHA256
        var result = new Dictionary<string, double>();
        if (string.IsNullOrEmpty(multipliers)) return result;
        
        foreach (var pair in multipliers.Split(','))
        {
            var parts = pair.Split('=');
            if (parts.Length == 2 && double.TryParse(parts[1], out var multiplier))
            {
                result[parts[0]] = multiplier;
            }
        }
        return result;
    }
}

// Tests/TestUtilities/FakeBenchmark/BenchmarkResultGenerator.cs  
public class BenchmarkResultGenerator
{
    private readonly BenchmarkConfiguration _config;
    private readonly Random _random = new(42); // Deterministic for testing
    
    public BenchmarkResultGenerator(BenchmarkConfiguration config)
    {
        _config = config;
    }
    
    public BenchmarkResult GenerateResults()
    {
        var title = $"{_config.BenchmarkName}-{DateTime.Now:yyyyMMdd-HHmmss}";
        
        return new BenchmarkResult
        {
            Title = title,
            HostEnvironmentInfo = GenerateHostInfo(),
            Benchmarks = GenerateBenchmarks()
        };
    }
    
    private List<Benchmark> GenerateBenchmarks()
    {
        // Generate realistic benchmark data based on the example JSON
        var benchmarks = new List<Benchmark>();
        
        // Create multiple parameter variations (N=1000, N=10000)
        foreach (var parameterSet in GetParameterSets())
        {
            var baselineTime = GetBaselineTime(parameterSet);
            var multiplier = _config.PerformanceMultipliers.GetValueOrDefault(
                parameterSet.MethodName, 1.0);
            
            var actualTime = baselineTime * multiplier;
            
            benchmarks.Add(new Benchmark
            {
                DisplayInfo = $"{_config.BenchmarkName}.{parameterSet.MethodName}: DefaultJob [{parameterSet.Parameters}]",
                Type = _config.BenchmarkName,
                Method = parameterSet.MethodName,
                Parameters = parameterSet.Parameters,
                FullName = $"{_config.BenchmarkName}.{parameterSet.MethodName}({parameterSet.Parameters})",
                Statistics = GenerateStatistics(actualTime),
                Memory = GenerateMemoryMetrics(parameterSet),
                Measurements = GenerateMeasurements(actualTime),
                Metrics = GenerateMetrics()
            });
        }
        
        return benchmarks;
    }
    
    private Statistics GenerateStatistics(double meanTime)
    {
        // Generate realistic statistical data around the mean
        var values = GenerateNormalDistribution(meanTime, meanTime * 0.03, 13);
        
        return new Statistics
        {
            OriginalValues = values,
            N = values.Count,
            Min = values.Min(),
            Mean = meanTime,
            Max = values.Max(),
            // Include all the statistical fields from real BenchmarkDotNet output
            StandardDeviation = meanTime * 0.03,
            // ... other statistical properties
        };
    }
}
```

##### Authentic JSON Structure
```csharp
// Tests/TestUtilities/FakeBenchmark/Models.cs
// These models match the exact structure of real BenchmarkDotNet JSON output

public class BenchmarkResult
{
    public string Title { get; set; }
    public HostEnvironmentInfo HostEnvironmentInfo { get; set; }
    public List<Benchmark> Benchmarks { get; set; }
}

public class HostEnvironmentInfo
{
    public string BenchmarkDotNetCaption { get; set; } = "BenchmarkDotNet";
    public string BenchmarkDotNetVersion { get; set; } = "0.14.0";
    public string OsVersion { get; set; } = "Linux";
    public string ProcessorName { get; set; } = "Fake CPU";
    public string RuntimeVersion { get; set; } = ".NET 9.0";
    // ... all other properties from real output
}

public class Benchmark
{
    public string DisplayInfo { get; set; }
    public string Type { get; set; }
    public string Method { get; set; }
    public string Parameters { get; set; }
    public string FullName { get; set; }
    public Statistics Statistics { get; set; }
    public MemoryInfo Memory { get; set; }
    public List<Measurement> Measurements { get; set; }
    public List<Metric> Metrics { get; set; }
}

// Complete all other classes to match the example JSON exactly...
```

#### 3.2 Test Repository Structure

Create test Git repositories with fake benchmark projects that use the fake BenchmarkDotNet implementation:

```
Tests/TestRepositories/
├── SimpleHashingRepo/
│   ├── .git/                     # Minimal Git history with known commits
│   ├── FakeBenchmarks/
│   │   ├── FakeBenchmarks.csproj # References FakeBenchmark as executable
│   │   └── Program.cs            # Simple console app that calls FakeBenchmark  
│   └── OtherProject/             # Non-benchmark project (ignored)
└── MultiBenchmarkRepo/
    ├── .git/
    ├── Algorithm.Benchmarks/     # Multiple benchmark projects
    │   ├── Algorithm.Benchmarks.csproj
    │   └── Program.cs            # Calls FakeBenchmark with FAKE_BENCHMARK_NAME=AlgorithmBenchmark
    └── Performance.Benchmarks/
        ├── Performance.Benchmarks.csproj  
        └── Program.cs            # Calls FakeBenchmark with FAKE_BENCHMARK_NAME=PerformanceBenchmark
```

##### Test Project Structure
```xml
<!-- Tests/TestRepositories/SimpleHashingRepo/FakeBenchmarks/FakeBenchmarks.csproj -->
<Project Sdk="Microsoft.NET.Sdk">
  <PropertyGroup>
    <OutputType>Exe</OutputType>
    <TargetFramework>net9.0</TargetFramework>
  </PropertyGroup>
  
  <ItemGroup>
    <ProjectReference Include="../../../TestUtilities/FakeBenchmark/FakeBenchmark.csproj" />
  </ItemGroup>
</Project>
```

```csharp
// Tests/TestRepositories/SimpleHashingRepo/FakeBenchmarks/Program.cs
using System;
using System.Threading.Tasks;

public static class Program
{
    public static async Task<int> Main(string[] args)
    {
        // Set environment variables to configure the fake benchmark
        Environment.SetEnvironmentVariable("FAKE_BENCHMARK_NAME", "HashingBenchmark");
        
        // Delegate to the fake BenchmarkDotNet implementation
        return await FakeBenchmark.Program.Main(args);
    }
}
```

#### 3.3 Performance Variation and Testing Scenarios

**Scenario Configuration**: Use environment variables to create different performance profiles for testing various comparison logic:

```csharp
// Tests/TestUtilities/FakeBenchmark/Scenarios.cs
public static class BenchmarkScenarios
{
    public static readonly Dictionary<string, Dictionary<string, double>> Scenarios = new()
    {
        ["baseline"] = new() 
        {
            ["Hash"] = 1.0,     // 1456.23 ns baseline from example
            ["MD5"] = 1.0,      
            ["SHA256"] = 1.0
        },
        
        ["performance-regression"] = new()
        {
            ["Hash"] = 1.15,    // 15% slower
            ["MD5"] = 1.2,      // 20% slower  
            ["SHA256"] = 1.1    // 10% slower
        },
        
        ["performance-improvement"] = new()
        {
            ["Hash"] = 0.9,     // 10% faster
            ["MD5"] = 0.85,     // 15% faster
            ["SHA256"] = 0.95   // 5% faster
        },
        
        ["mixed-results"] = new()
        {
            ["Hash"] = 0.9,     // Some faster
            ["MD5"] = 1.1,      // Some slower  
            ["SHA256"] = 1.0    // Some unchanged
        },
        
        ["high-variance"] = new()
        {
            ["Hash"] = 1.0,     // Same mean but higher variance in measurements
            ["MD5"] = 1.0,
            ["SHA256"] = 1.0
        }
    };
}
```

#### 3.4 End-to-End Test Implementation

**Complete CLI Workflow Testing** with configurable fake benchmarks:

```csharp
[Test]
public async Task InteractiveMode_PerformanceRegression_DetectedAndReported()
{
    // Arrange: Create test repo with different performance profiles per commit
    var testRepo = CreateTestRepository();
    await testRepo.CreateCommit("baseline", scenario: "baseline");
    await testRepo.CreateCommit("regression", scenario: "performance-regression");
    
    var tempDir = CreateTempDirectory();
    
    var args = new[]
    {
        "compare", "baseline", "regression",
        "--repo", testRepo.Path,
        "--benchmark", "FakeBenchmarks",
        "--output", tempDir.Path
    };
    
    // Act
    var exitCode = await Program.Main(args);
    
    // Assert
    Assert.That(exitCode, Is.EqualTo(0));
    
    // Verify comparison results show performance regression
    var outputFiles = Directory.GetFiles(tempDir.Path, "*report*");
    Assert.That(outputFiles, Has.Length.GreaterThan(0));
    
    var jsonReport = File.ReadAllText(outputFiles.First(f => f.EndsWith(".json")));
    var comparison = JsonSerializer.Deserialize<BenchmarkComparison>(jsonReport);
    
    // Verify regression is detected (15% slower in our scenario)
    var hashComparison = comparison.Results.First(r => r.Method == "Hash");
    Assert.That(hashComparison.PerformanceChange, Is.EqualTo(0.15).Within(0.01));
    Assert.That(hashComparison.IsRegression, Is.True);
}

[Test]
public async Task CiMode_MultipleBenchmarkProjects_AllExecuted()
{
    // Arrange: Directories with multiple benchmark projects
    var baselineDir = CreateTestDirectory("baseline")
        .WithFakeBenchmarkProject("Algorithm.Benchmarks", "AlgorithmBenchmark")
        .WithFakeBenchmarkProject("Performance.Benchmarks", "PerformanceBenchmark");
        
    var targetDir = CreateTestDirectory("target")
        .WithFakeBenchmarkProject("Algorithm.Benchmarks", "AlgorithmBenchmark") 
        .WithFakeBenchmarkProject("Performance.Benchmarks", "PerformanceBenchmark");
    
    var args = new[]
    {
        "ci", baselineDir.Path, targetDir.Path,
        "--benchmark", "Algorithm.Benchmarks", "Performance.Benchmarks"
    };
    
    // Act
    var exitCode = await Program.Main(args);
    
    // Assert
    Assert.That(exitCode, Is.EqualTo(0));
    
    // Verify both benchmark projects were executed
    var outputFiles = Directory.GetFiles("./", "*report*", SearchOption.AllDirectories);
    Assert.That(outputFiles.Any(f => f.Contains("AlgorithmBenchmark")), Is.True);
    Assert.That(outputFiles.Any(f => f.Contains("PerformanceBenchmark")), Is.True);
}

[Test]  
public async Task InteractiveMode_BenchmarkFailure_ReportsError()
{
    // Arrange: Configure fake benchmark to simulate failure
    Environment.SetEnvironmentVariable("FAKE_BENCHMARK_SHOULD_FAIL", "true");
    
    var testRepo = CreateTestRepository();
    await testRepo.CreateCommit("commit1");
    await testRepo.CreateCommit("commit2");
    
    var args = new[]
    {
        "compare", "commit1", "commit2", 
        "--repo", testRepo.Path,
        "--benchmark", "FakeBenchmarks"
    };
    
    // Act
    var exitCode = await Program.Main(args);
    
    // Assert  
    Assert.That(exitCode, Is.Not.EqualTo(0)); // Should fail
    
    // Clean up
    Environment.SetEnvironmentVariable("FAKE_BENCHMARK_SHOULD_FAIL", null);
}
```

#### 3.5 Test Repository Creation Utilities

**Helper classes for creating test scenarios**:

```csharp
// Tests/TestUtilities/TestRepository.cs
public class TestRepository : IDisposable
{
    public string Path { get; private set; }
    private readonly List<string> _commits = new();
    
    public static TestRepository Create()
    {
        var tempPath = System.IO.Path.GetTempPath();
        var repoPath = System.IO.Path.Combine(tempPath, $"test-repo-{Guid.NewGuid():N}");
        
        Directory.CreateDirectory(repoPath);
        
        // Initialize git repository
        Repository.Init(repoPath);
        
        return new TestRepository { Path = repoPath };
    }
    
    public async Task CreateCommit(string commitRef, string scenario = "baseline")
    {
        // Create fake benchmark project that uses the specified scenario
        var benchmarkDir = System.IO.Path.Combine(Path, "FakeBenchmarks");
        Directory.CreateDirectory(benchmarkDir);
        
        await CreateFakeBenchmarkProject(benchmarkDir, scenario);
        
        // Commit the changes
        using var repo = new Repository(Path);
        Commands.Stage(repo, "*");
        
        var signature = new Signature("Test User", "test@example.com", DateTimeOffset.Now);
        repo.Commit($"Commit {commitRef} with scenario {scenario}", signature, signature);
        
        _commits.Add(commitRef);
    }
    
    private async Task CreateFakeBenchmarkProject(string projectDir, string scenario)
    {
        var csprojContent = """
            <Project Sdk="Microsoft.NET.Sdk">
              <PropertyGroup>
                <OutputType>Exe</OutputType>
                <TargetFramework>net9.0</TargetFramework>
              </PropertyGroup>
              <ItemGroup>
                <ProjectReference Include="../../../TestUtilities/FakeBenchmark/FakeBenchmark.csproj" />
              </ItemGroup>
            </Project>
            """;
            
        var programContent = $"""
            using System;
            using System.Threading.Tasks;

            public static class Program
            {{
                public static async Task<int> Main(string[] args)
                {{
                    Environment.SetEnvironmentVariable("FAKE_BENCHMARK_SCENARIO", "{scenario}");
                    Environment.SetEnvironmentVariable("FAKE_BENCHMARK_NAME", "HashingBenchmark");
                    
                    return await FakeBenchmark.Program.Main(args);
                }}
            }}
            """;
            
        await File.WriteAllTextAsync(System.IO.Path.Combine(projectDir, "FakeBenchmarks.csproj"), csprojContent);
        await File.WriteAllTextAsync(System.IO.Path.Combine(projectDir, "Program.cs"), programContent);
    }
    
    public void Dispose()
    {
        if (Directory.Exists(Path))
        {
            Directory.Delete(Path, recursive: true);
        }
    }
}
```

**Key Integration Points**:

##### Environment Variable Configuration
```bash
# Test execution examples:

# Baseline performance (default)
FAKE_BENCHMARK_SCENARIO=baseline dotnet run

# Performance regression test
FAKE_BENCHMARK_SCENARIO=performance-regression dotnet run

# Custom performance multipliers 
FAKE_BENCHMARK_MULTIPLIERS="Hash=1.15,MD5=1.2" dotnet run

# Simulate benchmark failure
FAKE_BENCHMARK_SHOULD_FAIL=true dotnet run
```

##### Expected Output Compatibility
The fake BenchmarkDotNet produces JSON files that are **identical in structure** to real BenchmarkDotNet output:

```json
{
  "Title": "HashingBenchmark-20250720-123456",
  "HostEnvironmentInfo": { /* Realistic but fake host info */ },
  "Benchmarks": [
    {
      "DisplayInfo": "HashingBenchmark.Hash: DefaultJob [N=1000]",
      "FullName": "HashingBenchmark.Hash(N: 1000)",
      "Statistics": {
        "Mean": 1675.16724, // 15% slower than baseline 1456.23
        "StandardDeviation": 50.3,
        "Min": 1620.5,
        "Max": 1730.2
        // All other statistical fields...
      },
      "Memory": { /* Memory metrics */ },
      "Measurements": [ /* Individual measurements */ ],
      "Metrics": [ /* GC and threading metrics */ ]
    }
  ]
}
```

This ensures:
- **Benchy's JSON parsing** works identically with fake data
- **Statistical analysis** gets realistic input distributions  
- **Report generation** produces valid output formats
- **Error handling** can be tested with malformed JSON scenarios

### 4. Test Organization

```
Tests/
├── Benchy.Tests.Unit/           # Fast unit tests
│   ├── Core/
│   ├── Configuration/
│   ├── Output/
│   └── Infrastructure/
├── Benchy.Tests.Integration/    # Component integration tests
│   ├── Git/
│   ├── DotnetProjects/
│   └── Reporting/
├── Benchy.Tests.EndToEnd/       # Full CLI workflow tests
│   ├── InteractiveMode/
│   ├── CiMode/
│   └── OutputFormats/
├── TestUtilities/               # Shared test helpers
│   ├── TestGitRepository.cs
│   ├── FakeBenchmarkGenerator.cs
│   └── TempDirectoryManager.cs
└── TestData/
    ├── FakeBenchmarks/          # Fake benchmark projects
    ├── TestRepositories/        # Sample Git repositories
    └── SampleOutputs/           # Expected output files
```

### 5. Test Execution Strategy

#### 5.1 Development Workflow
- **Unit Tests**: Run on every build (< 1 second)
- **Integration Tests**: Run on commit (< 10 seconds)  
- **End-to-End Tests**: Run on PR (< 30 seconds with fake benchmarks)

#### 5.2 CI Pipeline
```yaml
# .github/workflows/test.yml
name: Tests
on: [push, pull_request]

jobs:
  test:
    runs-on: ubuntu-latest
    steps:
    - uses: actions/checkout@v4
    
    - name: Setup .NET
      uses: actions/setup-dotnet@v4
      with:
        dotnet-version: '9.x'
        
    - name: Unit Tests
      run: dotnet test Tests/Benchy.Tests.Unit/
      
    - name: Integration Tests  
      run: dotnet test Tests/Benchy.Tests.Integration/
      
    - name: End-to-End Tests
      run: dotnet test Tests/Benchy.Tests.EndToEnd/
```

### 6. Advanced Testing Scenarios

#### 6.1 Error Condition Testing
- Git repository errors (invalid refs, missing repos)
- Build failures (compilation errors, missing projects)
- Benchmark execution failures
- Invalid output parsing

#### 6.2 Performance Testing
- Memory usage with large repositories
- Cleanup of temporary directories
- Concurrent execution safety

#### 6.3 Cross-Platform Testing
- Windows/Linux/macOS compatibility
- Path handling differences
- Git behavior variations

### 7. Implementation Plan

1. **Phase 1**: Set up test infrastructure and unit tests
2. **Phase 2**: Create fake benchmark system and test repositories  
3. **Phase 3**: Implement integration tests with mocked dependencies
4. **Phase 4**: Build end-to-end tests using fake benchmarks
5. **Phase 5**: Add CI pipeline and cross-platform testing

### 8. Benefits of This Approach

- **Speed**: E2E tests run in seconds instead of minutes
- **Reliability**: Consistent fake results eliminate benchmark variability
- **Coverage**: Test error conditions impossible with real benchmarks
- **Isolation**: No dependency on external benchmark projects
- **Debugging**: Easier to reproduce and debug test failures
- **Scalability**: Can test many scenarios without time constraints

### 9. Alternative Implementation: Handler Testing

Instead of testing the full CLI, handlers can be tested directly:

```csharp
[Test]
public async Task InteractiveHandler_ValidArguments_CompletesSuccessfully()
{
    var handler = new InteractiveHandler();
    var args = new InteractiveHandler.Args
    {
        BaselineRef = "commit1",
        TargetRef = "commit2",
        RepositoryPath = testRepo.Directory,
        Benchmarks = new[] { "FakeBenchmarks" }
    };
    
    var result = await handler.Handle(args);
    
    Assert.That(result.IsSuccess, Is.True);
}
```

This approach provides:
- **Faster execution**: Skip CLI parsing overhead
- **Better error testing**: Direct access to handler exceptions
- **Simpler setup**: No need to construct CLI arguments
- **Focused testing**: Test business logic without CLI concerns