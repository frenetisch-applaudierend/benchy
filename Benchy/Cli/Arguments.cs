using System.CommandLine;

namespace Benchy.Cli;

public static class Arguments
{
    public static class Shared
    {
        public static readonly Option<string[]> BenchmarkOption = new(
            aliases: ["--benchmark", "-b"],
            description: "The benchmark project(s) to run",
            getDefaultValue: () => []
        );

        public static readonly Option<bool> VerboseOption = new(
            name: "--verbose",
            description: "Enable verbose output"
        );

        public static readonly Option<DirectoryInfo?> OutputDirectoryOption = new(
            name: "--output-directory",
            description: "The directory to output benchmark results"
        );

        public static readonly Option<string[]> OutputStyleOption = new Option<string[]>(
            name: "--output-style",
            description: "Output styles to use (comma-separated). Valid values: console, json, markdown. Default: console for interactive mode, json+markdown for CI mode",
            getDefaultValue: () => []
        ).WithValidOptions(["console", "json", "markdown"]);
    }

    public static class Interactive
    {
        public static readonly Option<DirectoryInfo?> RepositoryPathOption = new(
            aliases: ["--repository-path", "--repo", "-r"],
            description: "The path to the local Git repository (auto-detected from current directory if not specified)"
        );

        public static readonly Option<bool> NoDeleteOption = new(
            name: "--no-delete",
            description: "Do not delete the temporary directories after running the benchmarks"
        );

        public static readonly Argument<string> BaselineArgument = new(
            name: "baseline",
            description: "The commit reference (hash, branch, or tag) to the baseline version"
        );

        public static readonly Argument<string?> TargetArgument = new(
            name: "target",
            description: "The commit reference (hash, branch, or tag) to the target version (defaults to currently checked out version)"
        )
        {
            Arity = ArgumentArity.ZeroOrOne,
        };
    }

    public static class Ci
    {
        public static readonly Argument<DirectoryInfo> BaselineDirectoryArgument = new(
            name: "baseline",
            description: "The directory containing the baseline version to compare"
        );

        public static readonly Argument<DirectoryInfo> TargetDirectoryArgument = new(
            name: "target",
            description: "The directory containing the target version to compare"
        );
    }
}

file static class ArgumentsExtensions
{
    public static Option<T[]> WithValidOptions<T>(
        this Option<T[]> option,
        IReadOnlyCollection<T> validOptions
    )
    {
        option.AddValidator(result =>
        {
            var selectedOptions = result.GetValueForOption(option) ?? [];
            foreach (var selected in selectedOptions)
            {
                if (!validOptions.Contains(selected))
                {
                    result.ErrorMessage =
                        $"Invalid option: '{selected}'. Valid options are: {string.Join(", ", validOptions)}";
                    return;
                }
            }
        });
        return option;
    }
}
