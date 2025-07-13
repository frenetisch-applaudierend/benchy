using System.CommandLine;
using Benchy.Cli;

var rootCommand = new RootCommand("Benchmark Comparison Tool for comparing performance between commits");
rootCommand.AddOption(Arguments.Shared.VerboseOption);
rootCommand.AddOption(Arguments.Shared.BenchmarkOption);
rootCommand.AddOption(Arguments.Interactive.RepositoryPathOption);
rootCommand.AddOption(Arguments.Interactive.NoDeleteOption);
rootCommand.AddArgument(Arguments.Interactive.CommitsArgument);

var ciCommand = new Command("ci", "Compare benchmarks between pre-checked-out directories in a CI environment")
{
    IsHidden = true
};
ciCommand.AddOption(Arguments.Shared.BenchmarkOption);
ciCommand.AddOption(Arguments.Shared.VerboseOption);
ciCommand.AddArgument(Arguments.Ci.DirectoriesArgument);

rootCommand.AddCommand(ciCommand);

// Define command handlers
rootCommand.SetHandler(InteractiveHandler.Handle, Arguments.Shared.VerboseOption, Arguments.Shared.BenchmarkOption, Arguments.Interactive.RepositoryPathOption, Arguments.Interactive.NoDeleteOption, Arguments.Interactive.CommitsArgument);
ciCommand.SetHandler(CiHandler.Handle, Arguments.Shared.VerboseOption, Arguments.Shared.BenchmarkOption, Arguments.Ci.DirectoriesArgument);

// Execute the command
return await rootCommand.InvokeAsync(args);