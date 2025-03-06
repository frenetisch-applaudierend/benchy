using System.CommandLine;

// Create command-line arguments
var repoPathArg = new Argument<string>(
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
rootCommand.SetHandler((string repoPath, string baselineCommit, string comparisonCommit) =>
{
    Console.WriteLine($"Repository path: {repoPath}");
    Console.WriteLine($"Baseline commit: {baselineCommit}");
    Console.WriteLine($"Comparison commit: {comparisonCommit}");
    
    // TODO: Implement benchmark comparison logic
}, repoPathArg, baselineCommitArg, comparisonCommitArg);

// Execute the command
return await rootCommand.InvokeAsync(args);
