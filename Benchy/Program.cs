using System.CommandLine;
using Benchy.Cli;

var rootCommand = new RootCommand(
    "Benchmark Comparison Tool for comparing performance between commits"
);
rootCommand.AddGlobalOption(Arguments.Shared.VerboseOption);
rootCommand.AddGlobalOption(Arguments.Shared.OutputDirectoryOption);
rootCommand.AddGlobalOption(Arguments.Shared.BenchmarkOption);

var compareCommand = new Command(
    "compare",
    "Compare benchmarks between two commits in a repository"
);
compareCommand.AddOption(Arguments.Interactive.RepositoryPathOption);
compareCommand.AddOption(Arguments.Interactive.NoDeleteOption);
compareCommand.AddArgument(Arguments.Interactive.BaselineArgument);
compareCommand.AddArgument(Arguments.Interactive.TargetArgument);
rootCommand.AddCommand(compareCommand);

var ciCommand = new Command(
    "ci",
    "Compare benchmarks between pre-checked-out directories in a CI environment"
);
ciCommand.AddArgument(Arguments.Ci.BaselineDirectoryArgument);
ciCommand.AddArgument(Arguments.Ci.TargetDirectoryArgument);
rootCommand.AddCommand(ciCommand);

// Define command handlers
compareCommand.SetHandler(
    InteractiveHandler.Handle,
    Arguments.Shared.VerboseOption,
    Arguments.Shared.OutputDirectoryOption,
    Arguments.Shared.BenchmarkOption,
    Arguments.Interactive.RepositoryPathOption,
    Arguments.Interactive.NoDeleteOption,
    Arguments.Interactive.BaselineArgument,
    Arguments.Interactive.TargetArgument
);

ciCommand.SetHandler(
    CiHandler.Handle,
    Arguments.Shared.VerboseOption,
    Arguments.Shared.OutputDirectoryOption,
    Arguments.Shared.BenchmarkOption,
    Arguments.Ci.BaselineDirectoryArgument,
    Arguments.Ci.TargetDirectoryArgument
);

// Execute the command
return await rootCommand.InvokeAsync(args);
