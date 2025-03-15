namespace Benchy;

public sealed class BenchmarkVersion : IDisposable
{
    private readonly GitRepository _repository;

    private BenchmarkVersion(GitRepository repository)
    {
        _repository = repository;
    }

    ~BenchmarkVersion()
    {
        Dispose(false);
    }

    public static BenchmarkVersion CheckoutToTemporaryDirectory(GitRepository sourceRepository, string commitRef) {
        var targetPath = Path.Combine(Path.GetTempPath(), "src");

        var targetRepository = GitRepository.Clone(sourceRepository, targetPath);
        targetRepository.Checkout(commitRef);
        
        return new BenchmarkVersion(targetRepository);
    }

    void IDisposable.Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    private void Dispose(bool disposing)
    {
        _repository.Delete();

        if (disposing)
        {
            ((IDisposable)_repository).Dispose();
        }
    }
}
