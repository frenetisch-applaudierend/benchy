# Benchy

A .NET tool for comparing C# project benchmarks between different git commits.

## Features

- Compare BenchmarkDotNet results between git commits
- Interactive mode for local development
- CI mode for automated testing

## Installation

Install as a global .NET tool:

```bash
dotnet tool install --global benchy
```

Or install a specific version:

```bash
dotnet tool install --global benchy --version 1.0.0
```

## Usage

### Interactive Mode

Compare benchmarks between commits in a local repository:

```bash
# Compare current working directory against main branch
benchy compare main

# Compare two specific commits
benchy compare abc123 def456

# Specify repository path and benchmark projects
benchy compare main --repo /path/to/repo --benchmark MyBenchmarks

# Keep temporary directories for debugging
benchy compare main --no-delete
```

### CI Mode

For pre-checked-out directories in CI environments:

```bash
# Compare two directories containing different versions
benchy ci /path/to/baseline /path/to/target

# With specific benchmark selection
benchy ci baseline-dir target-dir --benchmark MyBenchmarks
```

### Options

- `--verbose`: Enable verbose output
- `--benchmark`, `-b`: Specify benchmark project(s) to run
- `--repository-path`, `--repo`, `-r`: Path to Git repository (auto-detected if not specified)
- `--no-delete`: Don't delete temporary directories after completion

## GitHub Action

Use the GitHub Action to automatically compare benchmarks in pull requests:

```yaml
name: Benchmark Comparison
on:
  pull_request:
    branches: [ main ]

jobs:
  benchmark:
    runs-on: ubuntu-latest
    steps:
    - uses: actions/checkout@v4
    
    - name: Run Benchmark Comparison
      uses: frenetisch-applaudierend/benchy@v1
      with:
        baseline-ref: main
        benchy-version: latest
        dotnet-version: '9.x'
```

### Action Inputs

- `baseline-ref`: Git reference for baseline comparison (default: main branch)
- `benchy-version`: Version of Benchy tool to install (default: 'latest')
- `dotnet-version`: .NET version to use (default: '9.x')

### Action Outputs

- `comparison-result`: Path to the benchmark comparison results directory

## Requirements

- .NET 9.0 or later
- Git repository with BenchmarkDotNet projects
- BenchmarkDotNet configured in your C# projects
