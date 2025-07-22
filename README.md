# Benchy

A .NET tool for comparing [BenchmarkDotNet](https://github.com/dotnet/BenchmarkDotNet)
benchmarks between different git commits. It is intended to keep track of performance changes
in your codebase, making it easier to identify regressions or improvements.

## Features

- Interactive mode for local development
- CI mode with associated github action for automated testing

## Local Installation

Install as a global .NET tool:

```bash
dotnet tool install --global FrenetischApplaudierend.Benchy
```

And run it with:

```bash
benchy --help
```

Or install locally in your project:

```bash
dotnet tool install --local FrenetischApplaudierend.Benchy
```

And run it with:

```bash
dotnet benchy --help
```

## Interactive Usage

To use the tool interactively, run `benchy compare <baseline> [<target>] [options]`.
`baseline` and `target` are git references (branches, tags, commits). If no target is specified,
the current working copy (including uncommitted changes) is used as the target.

### Options

> [!NOTE]
> Only the most relevant options are shown here. For a full list, run `benchy compare --help`.

| Option                                 | Description                                                                             |
| -------------------------------------- | --------------------------------------------------------------------------------------- |
| `--verbose`                            | Enable verbose output                                                                   |
| `--significance-threshold <threshold>` | Statistical significance threshold for benchmark comparisons (0.0-1.0, default: 0.05)   |
| `-b`, `--benchmark <benchmark>`        | The benchmark project(s) to run. Specify the flag multiple times for multiple projects. |

Benchmarks can be either specified as the path to a `.csproj` file or as the name of a project,
i.e. `MyBenchmarkProject.csproj` or `MyBenchmarkProject`. In the latter case, Benchy will look
for the project in `<Repo>/MyBenchmarkProject.csproj` and `<Repo>/MyBenchmarkProject/MyBenchmarkProject.csproj`.

> [!IMPORTANT]
> Benchy configures the specified benchmark projects with command line arguments, so in order to
> support Benchy, your benchmark project's `Main` method must pass command line arguments to
> `BenchmarkRunner`:
> ```csharp
> static void Main(string[] args)
> {
>     BenchmarkRunner.Run<MyBenchmark>(args: args);
> }
> ```

These options can also be specified in a [configuration file](#configuration-file).

### Examples

```bash
# Compare two branches with specified benchmarks
benchy compare main my-feature-branch \
  --benchmark Path/To/MyBenchmarkProject.csproj \
  --benchmark AnotherBenchmark

# Compare the current working copy to the main branch, read benchmarks from config file
benchy compare main
```

## GitHub Action

To use Benchy as a GitHub Action, use: `frenetisch-applaudierend/benchy@v1` from the
[actions marketplace](https://github.com/marketplace/actions/benchy-benchmark-comparison) in
your workflow file.

The action compares the benchmarks between the revision in the current branch (i.e. the
pull request) and a baseline reference. After running the benchmarks, it puts the
comparison as a comment on the pull request.

### Action Inputs

| Input            | Description                                               |
| ---------------- | --------------------------------------------------------- |
| `baseline-ref`   | Git reference for baseline comparison                     |
| `benchmarks`     | Array of benchmark names to run as comma-separated string |
| `benchy-version` | Version of Benchy tool to install or use 'latest'         |
| `dotnet-version` | .NET version to use for benchmarks                        |

All inputs are optional. Defaults are as follows:

- `baseline-ref`: The repository default branch
- `benchmarks`: Benchmarks are read from the configuration file if missing
- `benchy-version`: Same as the action version
- `dotnet-version`: 9.x (latest LTS)

### Action Outputs

- `comparison-result`: Path to the benchmark comparison results directory

### Required Permissions

The action requires the following permissions on the github token:

- `contents: read`: To check out different revisions of the code
- `pull-requests: write`: To post comments on the pull request

### Example Workflow

```yaml
name: Benchmark Comparison
on:
  pull_request:
    branches: [ main ]
  workflow_dispatch:

permissions:
  contents: read  # Required for checking out code
  pull-requests: write  # Required for PR comments

jobs:
  benchmark:
    runs-on: ubuntu-latest
    steps:
    - name: Run Benchmark Comparison
      uses: frenetisch-applaudierend/benchy@v1
      with:
        benchmarks: "Path/To/MyBenchmarkProject.csproj, AnotherBenchmark"
```

## CI Mode

Benchy has a different command to support CI scenarios, like the GitHub Action. The main
difference is that in this mode, the tool operates on pre-checked out directories instead
of git references. See `benchy ci --help` for more information on how to use it.

## Configuration File

To specify benchmarks and other options that should always be the same, you can create a
configuration file in [TOML format](https://toml.io). Benchy will automatically read the
configuration from the following locations (first match is used):

- `<Repo>/.config/benchy.toml`
- `<Repo>/benchy.toml`
- `<Repo>/.benchy.toml`

### Available Configuration Options

The configuration file supports hierarchical settings with global defaults and mode-specific
overrides. Command line arguments take the highest precedence, followed by mode-specific
settings, then global settings.

| Option                   | Type             | Description                                                 |
| ------------------------ | ---------------- | ----------------------------------------------------------- |
| `verbose`                | boolean          | Enable verbose output                                       |
| `output_directory`       | string           | Directory to output benchmark results                       |
| `output_style`           | array of strings | Output formats: `console`, `json`, `markdown`               |
| `significance_threshold` | number           | Statistical significance threshold (0.0-1.0, default: 0.05) |
| `benchmarks`             | array of strings | Benchmark projects to run                                   |
| `no_delete`              | boolean          | Don't delete temporary directories (for debugging)          |

### Mode-Specific Configuration

You can specify different settings for Interactive and CI modes using `[interactive]` and `[ci]`
sections. These override the global settings for their respective modes.

### Example Configuration File

```toml
# Global settings (apply to all modes unless overridden)
verbose = false
output_directory = "./benchmark-results"
output_style = ["console"]
significance_threshold = 0.05  # 5% threshold for significant changes
benchmarks = [
    "MyProject.Benchmarks",
    "MyProject.PerformanceTests"
]

# Settings specific to interactive mode (benchy compare)
[interactive]
output_style = ["console"]
no_delete = false
benchmarks = [
    "MyProject.Benchmarks",
    "MyProject.InteractiveBenchmarks"
]

# Settings specific to CI mode (benchy ci / GitHub Action)
[ci]
output_style = ["json", "markdown"]
verbose = true
no_delete = true
significance_threshold = 0.03  # Stricter 3% threshold for CI
benchmarks = [
    "MyProject.Benchmarks", 
    "MyProject.StabilityTests"
]
```
