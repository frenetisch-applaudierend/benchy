using System.CommandLine;
using Benchy;

// Create command-line arguments
var repoPathArg = new Argument<DirectoryInfo>(
    name: "repository-path",
    description: "The path to the local Git repository");

var commitsArg = new Argument<string[]>(
    name: "commit-refs",
    description: "The commit references (hash, branch, or tag) to compare against each other");

var benchmarkOption = new Option<string[]>(
    aliases: ["--benchmark", "-b"],
    description: "The benchmark(s) to run",
    getDefaultValue: () => []);

var noDeleteOption = new Option<bool>("--no-delete", "Do not delete the temporary directories after running the benchmarks");

var verboseOption = new Option<bool>("--verbose", "Enable verbose output");

// Create the root command
var rootCommand = new RootCommand("Benchmark Comparison Tool for comparing performance between commits");
rootCommand.AddArgument(repoPathArg);
rootCommand.AddArgument(commitsArg);
rootCommand.AddGlobalOption(benchmarkOption);
rootCommand.AddGlobalOption(noDeleteOption);
rootCommand.AddGlobalOption(verboseOption);

// Define the command handler
rootCommand.SetHandler(BenchmarkComparer.RunAndCompareBenchmarks, repoPathArg, commitsArg, benchmarkOption, noDeleteOption, verboseOption);

// Execute the command
return await rootCommand.InvokeAsync(args);