using Benchy.Infrastructure;
using Benchy.Output;

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

    public void Build(bool verbose)
    {
        CliOutput.Info($"Building benchmark project: {project.Name}", indent: 1);
        project.Build(verbose);
    }

    public void Run(DirectoryInfo outputDirectory, bool verbose)
    {
        CliOutput.Info($"Running benchmark project: {project.Name}", indent: 1);
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
