using System.CommandLine;

var repoPathArgument = new Argument<string>(
    "repo-path",
    "The path to the local Git repository"
);

var commitHash1Argument = new Argument<string>(
    "commit-hash-1",
    "The first commit hash"
);

var commitHash2Argument = new Argument<string>(
    "commit-hash-2",
    "The second commit hash"
);

var rootCommand = new RootCommand
{
    repoPathArgument,
    commitHash1Argument,
    commitHash2Argument
};

rootCommand.Description = "Benchmark Comparison Tool";
rootCommand.SetHandler((repoPath, commitHash1, commitHash2) =>
{
    Console.WriteLine($"Repository Path: {repoPath}");
    Console.WriteLine($"Commit Hash 1: {commitHash1}");
    Console.WriteLine($"Commit Hash 2: {commitHash2}");
    // ...additional logic to handle the arguments...
}, repoPathArgument, commitHash1Argument, commitHash2Argument);

return rootCommand.InvokeAsync(args).Result;
