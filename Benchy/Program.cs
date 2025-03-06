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

// Create the root command
var rootCommand = new RootCommand("Benchmark Comparison Tool for comparing performance between commits");
rootCommand.AddArgument(repoPathArg);
rootCommand.AddArgument(baselineCommitArg);
rootCommand.AddArgument(comparisonCommitArg);

// Define the command handler
rootCommand.SetHandler((repoPath, baselineCommit, comparisonCommit) =>
{
    try
    {
        // Create benchmark comparer instance using the factory method
        var comparer = BenchmarkComparer.Create(repoPath);
        
        // Run the benchmark comparison with the commit references
        comparer.RunComparison(baselineCommit, comparisonCommit);
    }
    catch (BenchmarkComparisonException ex)
    {
        Console.Error.WriteLine($"Benchmark comparison error: {ex.Message}");
        Environment.Exit(1);
    }
}, repoPathArg, baselineCommitArg, comparisonCommitArg);

// Execute the command
return await rootCommand.InvokeAsync(args);
