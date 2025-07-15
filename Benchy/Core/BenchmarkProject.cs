using Benchy.Infrastructure;

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
        Output.Info($"Building benchmark project: {project.Name}", indent: 1);
        project.Build(verbose);
    }

    public void Run(DirectoryInfo outputDirectory, bool verbose)
    {
        Output.Info($"Running benchmark project: {project.Name}", indent: 1);
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
