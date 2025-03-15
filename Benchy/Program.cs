using System.CommandLine;
using Benchy;

// Create command-line arguments
var repoPathArg = new Argument<DirectoryInfo>(
    name: "repository-path",
    description: "The path to the local Git repository");

var baselineCommitArg = new Argument<string>(
    name: "baseline-commit",
    description: "The baseline commit reference (hash, branch, or tag)");

var comparisonCommitArg = new Argument<string>(
    name: "comparison-commit",
    description: "The comparison commit reference (hash, branch, or tag)");

var noDeleteOption = new Option<bool>("--no-delete", "Do not delete the temporary directories after running the benchmarks");

// Create the root command
var rootCommand = new RootCommand("Benchmark Comparison Tool for comparing performance between commits");
rootCommand.AddArgument(repoPathArg);
rootCommand.AddArgument(baselineCommitArg);
rootCommand.AddArgument(comparisonCommitArg);
rootCommand.AddOption(noDeleteOption);

// Define the command handler
rootCommand.SetHandler(BenchmarkComparer.RunAndCompareBenchmarks, repoPathArg, baselineCommitArg, comparisonCommitArg, noDeleteOption);

// Execute the command
return await rootCommand.InvokeAsync(args);
