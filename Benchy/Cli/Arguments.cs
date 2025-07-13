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
        
        public static readonly Argument<string[]> CommitsArgument = new(
            name: "commit-refs",
            description: "The commit references (hash, branch, or tag) to compare against each other")
        {
            Arity = ArgumentArity.OneOrMore
        };

        static Interactive()
        {
            CommitsArgument.AddValidator(result =>
            {
                var commits = result.GetValueForArgument(CommitsArgument);
                if (commits.Length < 2)
                {
                    result.ErrorMessage = "At least 2 commit references are required.";
                }
            });
        }
    }

    public static class Ci
    {
        public static readonly Argument<DirectoryInfo[]> DirectoriesArgument = new(
            name: "directories",
            description: "The directories containing the code versions to compare")
        {
            Arity = ArgumentArity.OneOrMore
        };

        static Ci()
        {
            DirectoriesArgument.AddValidator(result =>
            {
                var directories = result.GetValueForArgument(DirectoriesArgument);
                if (directories.Length < 2)
                {
                    result.ErrorMessage = "At least 2 directories are required.";
                }
            });
        }
    }
    
}
