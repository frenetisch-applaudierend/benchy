using System.CommandLine;

namespace Benchy.Cli;

public static class Arguments
{
    public static class Shared
    {
        public static readonly Option<string[]> BenchmarkOption = new(
            aliases: ["--benchmark", "-b"],
            description: "The benchmark(s) to run",
            getDefaultValue: () => []);

        public static readonly Option<bool> VerboseOption = new(
            name: "--verbose",
            description: "Enable verbose output");
    }

    public static class Interactive
    {
        public static readonly Option<DirectoryInfo?> RepositoryPathOption = new(
            aliases: ["--repository-path", "--repo", "-r"],
            description: "The path to the local Git repository (auto-detected from current directory if not specified)");

        public static readonly Option<bool> NoDeleteOption = new(
            name: "--no-delete",
            description: "Do not delete the temporary directories after running the benchmarks");
        
        public static readonly Argument<string[]> CommitsArgument = new Argument<string[]>(
            name: "commit-refs",
            description: "The commit references (hash, branch, or tag) to compare against each other")
            .WithMultipleItems(minCount: 2);
    }

    public static class Ci
    {
        public static readonly Argument<DirectoryInfo[]> DirectoriesArgument = new Argument<DirectoryInfo[]>(
            name: "directories",
            description: "The directories containing the code versions to compare")
            .WithMultipleItems(minCount: 2);
    }
    
}

file static class ArgumentsExtensions
{
    public static Argument<T[]> WithMultipleItems<T>(this Argument<T[]> argument, int minCount)
    {
        argument.Arity = ArgumentArity.OneOrMore;
        argument.AddValidator(result =>
            {
                var commits = result.GetValueForArgument(argument);
                if (commits.Length < minCount)
                {
                    result.ErrorMessage = $"At least {minCount} {argument.Name} are required.";
                }
            });

        return argument;
    }
}