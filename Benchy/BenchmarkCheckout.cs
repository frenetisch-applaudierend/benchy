namespace Benchy;

public sealed class BenchmarkVersion : IDisposable
{
    private readonly GitRepository _repository;

    private BenchmarkVersion(GitRepository repository)
    {
        _repository = repository;
    }

    public static BenchmarkVersion CheckoutToTemporaryDirectory(GitRepository sourceRepository, string commitRef)
    {
        var targetPath = Path.Combine(Path.GetTempPath(), "src");

        var targetRepository = GitRepository.Clone(sourceRepository, targetPath);
        targetRepository.Checkout(commitRef);

        return new BenchmarkVersion(targetRepository);
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
