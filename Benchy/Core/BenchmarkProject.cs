using Benchy.Infrastructure;
using Benchy.Output;
using static Benchy.Output.FormattedText;

namespace Benchy.Core;

public sealed record BenchmarkProject
{
    private readonly DotnetProject project;

    private BenchmarkProject(DotnetProject project)
    {
        this.project = project;
    }

    public static BenchmarkProject FromName(string name, DirectoryInfo sourceDirectory)
    {
        var projectFile = sourceDirectory.File(name);

        return new BenchmarkProject(DotnetProject.Open(projectFile));
    }

    public string Name => project.Name;

    public void Build(bool verbose)
    {
        project.Build(verbose);
    }

    public void Run(DirectoryInfo outputDirectory, bool verbose)
    {
        project.Run(
            [
                "--keepFiles",
                "--stopOnFirstError",
                "--memory",
                "--threading",
                "--exporters",
                "JSON",
                "--artifacts",
                outputDirectory.FullName,
            ],
            verbose
        );
    }
}
