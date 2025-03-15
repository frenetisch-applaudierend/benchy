namespace Benchy;

public sealed class BenchmarkVersion : IDisposable
{
    private readonly GitRepository _repository;

    private BenchmarkVersion(GitRepository repository, string commitRef)
    {
        _repository = repository;
        CommitRef = commitRef;
    }

    public string CommitRef { get; }

    public static BenchmarkVersion CheckoutToTemporaryDirectory(GitRepository sourceRepository, string commitRef)
    {
        var targetPath = Path.Combine(Path.GetTempPath(), "Benchy", Path.GetRandomFileName());

        var targetRepository = GitRepository
            .Clone(sourceRepository, targetPath)
            .Checkout(commitRef);

        return new BenchmarkVersion(targetRepository, commitRef);
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
