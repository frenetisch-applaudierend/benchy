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
}
