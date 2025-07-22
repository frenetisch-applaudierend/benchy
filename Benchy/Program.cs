using System.CommandLine;
using Benchy.Cli;

var rootCommand = new RootCommand(
    "Benchmark Comparison Tool for comparing performance between commits"
);
rootCommand.AddGlobalOption(Arguments.Shared.VerboseOption);
rootCommand.AddGlobalOption(Arguments.Shared.OutputDirectoryOption);
rootCommand.AddGlobalOption(Arguments.Shared.OutputStyleOption);
rootCommand.AddGlobalOption(Arguments.Shared.BenchmarkOption);
rootCommand.AddGlobalOption(Arguments.Shared.SignificanceThresholdOption);
rootCommand.AddGlobalOption(Arguments.Shared.DecorationOption);

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
    (context) =>
    {
        InteractiveHandler.Handle(
            context.ParseResult.GetValueForOption(Arguments.Shared.VerboseOption),
            context.ParseResult.GetValueForOption(Arguments.Shared.OutputDirectoryOption),
            context.ParseResult.GetValueForOption(Arguments.Shared.OutputStyleOption),
            context.ParseResult.GetValueForOption(Arguments.Shared.BenchmarkOption),
            context.ParseResult.GetValueForOption(Arguments.Shared.SignificanceThresholdOption),
            context.ParseResult.GetValueForOption(Arguments.Shared.DecorationOption),
            context.ParseResult.GetValueForOption(Arguments.Interactive.RepositoryPathOption),
            context.ParseResult.GetValueForOption(Arguments.Interactive.NoDeleteOption),
            context.ParseResult.GetValueForArgument(Arguments.Interactive.BaselineArgument),
            context.ParseResult.GetValueForArgument(Arguments.Interactive.TargetArgument)
        );
    }
);

ciCommand.SetHandler(
    (context) =>
    {
        CiHandler.Handle(
            context.ParseResult.GetValueForOption(Arguments.Shared.VerboseOption),
            context.ParseResult.GetValueForOption(Arguments.Shared.OutputDirectoryOption),
            context.ParseResult.GetValueForOption(Arguments.Shared.OutputStyleOption),
            context.ParseResult.GetValueForOption(Arguments.Shared.BenchmarkOption),
            context.ParseResult.GetValueForOption(Arguments.Shared.SignificanceThresholdOption),
            context.ParseResult.GetValueForOption(Arguments.Shared.DecorationOption),
            context.ParseResult.GetValueForArgument(Arguments.Ci.BaselineDirectoryArgument),
            context.ParseResult.GetValueForArgument(Arguments.Ci.TargetDirectoryArgument)
        );
    }
);

return await rootCommand.InvokeAsync(args);
