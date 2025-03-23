namespace Benchy;

public sealed class BenchmarkVersion : IDisposable
{
    private readonly GitRepository _repository;
    private readonly Dictionary<string, DotnetProject> _benchmarkProjects = [];

    private BenchmarkVersion(GitRepository repository, string commitRef, DirectoryInfo directory)
    {
        _repository = repository;
        CommitRef = commitRef;
        Directory = directory;
        SourceDirectory = new DirectoryInfo(Path.Combine(directory.FullName, "src"));
        OutputDirectory = new DirectoryInfo(Path.Combine(directory.FullName, "out"));
    }

    public string CommitRef { get; }
    public DirectoryInfo Directory { get; }
    public DirectoryInfo SourceDirectory { get; }
    public DirectoryInfo OutputDirectory { get; }

    public static BenchmarkVersion CheckoutToTemporaryDirectory(GitRepository sourceRepository, string commitRef)
    {
        var versionDirectoryName = $"{Sanitize(commitRef)}_{Path.GetRandomFileName()}";
        var directoryPath = Path.Combine(Path.GetTempPath(), "Benchy", versionDirectoryName);
        var repositoryPath = Path.Combine(directoryPath, "src");

        var targetRepository = GitRepository
            .Clone(sourceRepository, repositoryPath)
            .Checkout(commitRef);

        return new BenchmarkVersion(targetRepository, commitRef, new DirectoryInfo(directoryPath));
    }

    private static string Sanitize(string fileNamePart) => string.Join("_", fileNamePart.Split(Path.GetInvalidFileNameChars()));

    public DotnetProject OpenBenchmarkProject(string projectPath)
    {
        if (!_benchmarkProjects.TryGetValue(projectPath, out var project))
        {
            project = DotnetProject.Open(Path.Combine(SourceDirectory.FullName, projectPath));
            _benchmarkProjects[projectPath] = project;
        }
        return project;
    }

    public void Delete()
    {
        _repository.Delete();
    }

    void IDisposable.Dispose()
    {
        ((IDisposable)_repository).Dispose();
    }
}
