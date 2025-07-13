# Benchy - Benchmark Comparison Tool Architecture

## Project Overview

**Benchy** is a .NET 9.0 console application designed to compare BenchmarkDotNet performance benchmarks between different Git commits. It automates the process of checking out commits, building benchmark projects, running benchmarks, and analyzing the results to identify performance regressions or improvements.

## Project Structure

```
Benchy/
├── Benchy.sln                    # Visual Studio solution file
├── global.json                   # .NET SDK version configuration
├── Specification.md               # Detailed project requirements
├── Examples/                      # Example repositories and documentation
│   ├── hashing.git/              # Sample Git repository for testing
│   └── hashing.md                # Example usage documentation
├── Documentation/                 # Project documentation
│   └── Architecture.md           # This file
└── Benchy/                       # Main application source code
    ├── Benchy.csproj             # Project file
    ├── Program.cs                # Entry point and CLI setup
    ├── BenchmarkComparer.cs      # Main orchestration logic
    ├── BenchmarkCheckout.cs      # Git checkout and version management
    ├── BenchmarkResult.cs        # Result data structures
    ├── BenchmarkReport.cs        # Report parsing and data models
    ├── GitRepository.cs          # Git operations wrapper
    ├── DotnetProject.cs          # .NET project build/run operations
    ├── DirectoryInfoExtensions.cs # File system utilities
    ├── Output.cs                 # Console output formatting
    └── Cli/                      # Command-line interface components
        ├── Arguments.cs          # CLI arguments and options definitions
        ├── InteractiveHandler.cs # Interactive mode command handler
        └── CiHandler.cs          # CI mode command handler
```

## Core Components

### 1. Command Line Interface (`Program.cs` + `Cli/`)
- **Purpose**: Entry point and CLI structure using System.CommandLine with two operating modes
- **Structure**: Modular CLI components in the `Benchy.Cli` namespace
- **Operating Modes**:
  - **Interactive Mode (Default)**: Git-based workflow for local development
    - Takes commit references as arguments
    - Auto-detects repository from current directory or uses `--repository-path`
    - Usage: `benchy commit1 commit2 --benchmark MyBench`
  - **CI Mode**: Directory-based workflow for pre-checked-out code
    - Takes directory paths as arguments 
    - Usage: `benchy ci /path/dir1 /path/dir2 --benchmark MyBench`
- **Shared Options**:
  - `--benchmark/-b`: Specific benchmarks to run
  - `--verbose`: Enable detailed output
- **Interactive-specific Options**:
  - `--repository-path/--repo/-r`: Git repository path (auto-detected if not specified)
  - `--no-delete`: Preserve temporary directories after execution
- **Dependencies**: System.CommandLine library for argument parsing

#### CLI Components (`Cli/`)
- **`Arguments.cs`**: Centralized argument and option definitions organized by mode
  - `Arguments.Shared`: Options available in both modes
  - `Arguments.Interactive`: Interactive mode specific arguments/options
  - `Arguments.Ci`: CI mode specific arguments
- **`InteractiveHandler.cs`**: Handles interactive mode command processing
- **`CiHandler.cs`**: Handles CI mode command processing

### 2. Benchmark Orchestrator (`BenchmarkComparer.cs`)
- **Purpose**: Main business logic coordinator that manages the entire benchmark comparison workflow
- **Key Methods**:
  - `RunAndCompareBenchmarks()`: Primary entry point
  - `PrepareComparison()`: Sets up each commit version
  - `RunBenchmarks()`: Executes benchmarks for a version
  - `AnalyzeBenchmarks()`: Processes and displays results
  - `Cleanup()`: Removes temporary files
- **Workflow**:
  1. Validates input arguments
  2. Creates temporary checkout directories for each commit
  3. Builds benchmark projects
  4. Runs benchmarks with BenchmarkDotNet
  5. Collects and analyzes results
  6. Cleans up temporary resources

### 3. Version Management (`BenchmarkCheckout.cs`)
- **Purpose**: Manages Git checkout operations and temporary directory lifecycle
- **Key Features**:
  - Creates isolated temporary directories for each commit
  - Clones source repository to avoid affecting the original
  - Manages directory structure (`src/`, `out/`)
  - Handles project discovery and caching
  - Provides cleanup functionality
- **Directory Structure**:
  ```
  /tmp/Benchy/{sanitized_commit_ref}_{random}/
  ├── src/          # Cloned repository source code
  └── out/          # Benchmark output and artifacts
  ```

### 4. Git Operations (`GitRepository.cs`)
- **Purpose**: Wrapper around LibGit2Sharp for Git operations
- **Capabilities**:
  - Repository opening and validation
  - Repository cloning
  - Commit checkout by reference (hash, branch, tag)
  - Repository cleanup
- **Dependencies**: LibGit2Sharp for Git operations

### 5. .NET Project Management (`DotnetProject.cs`)
- **Purpose**: Handles .NET project building and execution
- **Operations**:
  - Project file discovery (supports both explicit .csproj paths and project names)
  - Release configuration builds
  - Benchmark execution with custom arguments
  - Process management and error handling
- **Build Command**: `dotnet build "{project}" --configuration Release`
- **Run Command**: `dotnet run --project "{project}" --no-build --configuration Release -- {args}`

### 6. Benchmark Reporting (`BenchmarkReport.cs`)
- **Purpose**: Parses and models BenchmarkDotNet JSON output
- **Data Model**:
  - `BenchmarkReport`: Contains title and list of benchmarks
  - `Benchmark`: Individual benchmark with full name and statistics
  - `Statistics`: Performance metrics (currently only Mean)
- **File Processing**: Loads `*report-full-compressed.json` files from results directory
- **Serialization**: Uses System.Text.Json for deserialization

### 7. Result Management (`BenchmarkResult.cs`)
- **Purpose**: Simple record linking benchmark versions to their reports
- **Structure**: Links `BenchmarkVersion` with `IReadOnlyList<BenchmarkReport>`

### 8. Utilities
- **`DirectoryInfoExtensions.cs`**: Extension methods for file system operations
  - `SubDirectory()`: Creates subdirectory paths
  - `File()`: Creates file paths
- **`Output.cs`**: Console output management
  - Supports different output levels (Info, Verbose, Error)
  - Provides indentation for hierarchical output
  - Respects verbose mode configuration

## Dependencies

### NuGet Packages
- **LibGit2Sharp (0.27.2)**: Git operations and repository management
- **System.CommandLine (2.0.0-beta4.22272.1)**: Modern command-line interface framework

### Framework
- **.NET 9.0**: Target framework
- **C# 12**: Language features (records, using declarations, etc.)

## Benchmark Integration

### BenchmarkDotNet Integration
- **Execution**: Runs benchmarks using `dotnet run` with specific arguments
- **Arguments Passed to Benchmarks**:
  - `--keepFiles`: Preserves benchmark artifacts
  - `--stopOnFirstError`: Fails fast on errors
  - `--memory`: Includes memory allocation metrics
  - `--threading`: Includes threading information
  - `--exporters JSON`: Outputs results in JSON format
  - `--artifacts {output_dir}`: Specifies output directory

### Output Processing
- Expects BenchmarkDotNet to generate `*report-full-compressed.json` files
- Currently parses only Mean statistics from benchmark results
- Results are organized by commit and benchmark report

## Workflow Examples

### Interactive Mode
1. **Input**: `benchy algs/md5 algs/sha256 -b ExampleBenchmark`
2. **Setup**: Auto-detect repository from current directory, create temporary directories for each commit
3. **Checkout**: Clone repository and checkout each commit to temporary locations
4. **Build**: Execute `dotnet build` for ExampleBenchmark project in each commit
5. **Execute**: Run benchmarks with BenchmarkDotNet arguments
6. **Collect**: Parse JSON results from each execution
7. **Analyze**: Display performance metrics comparison
8. **Cleanup**: Remove temporary directories (unless `--no-delete` specified)

### CI Mode
1. **Input**: `benchy ci /checkout/baseline /checkout/feature -b ExampleBenchmark`
2. **Setup**: Use pre-checked-out directories directly (no Git operations)
3. **Build**: Execute `dotnet build` for ExampleBenchmark project in each directory
4. **Execute**: Run benchmarks with BenchmarkDotNet arguments
5. **Collect**: Parse JSON results from each execution
6. **Analyze**: Display performance metrics comparison

## Error Handling

- **Validation**: Ensures minimum two commits and at least one benchmark specified
- **Git Errors**: Validates repository path and commit references
- **Build Failures**: Reports .NET build errors with optional verbose output
- **Execution Failures**: Handles benchmark runtime errors
- **Cleanup**: Guarantees resource cleanup even on failures

## Configuration

- **global.json**: Specifies .NET SDK version (9.0.0) with rollForward policy for version flexibility
- **Project Settings**: Configured for self-contained, single-file publishing
- **Release Configuration**: All builds and executions use Release configuration for accurate performance measurements

## Extensibility Points

- **Statistics Parsing**: Currently limited to Mean; can be extended for other BenchmarkDotNet metrics
- **Export Formats**: JSON export is hardcoded; could support other BenchmarkDotNet exporters
- **Comparison Logic**: Basic result display; could be enhanced with statistical analysis
- **Project Discovery**: Simple .csproj-based; could support more complex project structures