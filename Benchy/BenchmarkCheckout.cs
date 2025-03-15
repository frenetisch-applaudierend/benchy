namespace Benchy;

public sealed class BenchmarkVersion : IDisposable
{
    private readonly GitRepository _repository;

    private BenchmarkVersion(GitRepository repository, string commitRef, DirectoryInfo directory)
    {
        _repository = repository;
        CommitRef = commitRef;
        Directory = directory;
    }

    public string CommitRef { get; }
    public DirectoryInfo Directory { get; internal set; }

    public static BenchmarkVersion CheckoutToTemporaryDirectory(GitRepository sourceRepository, string commitRef)
    {
        var directoryPath = Path.Combine(Path.GetTempPath(), "Benchy", $"{commitRef}_{Path.GetRandomFileName()}");
        var repositoryPath = Path.Combine(directoryPath, "src");

        var targetRepository = GitRepository
            .Clone(sourceRepository, repositoryPath)
            .Checkout(commitRef);

        return new BenchmarkVersion(targetRepository, commitRef, new DirectoryInfo(directoryPath));
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
