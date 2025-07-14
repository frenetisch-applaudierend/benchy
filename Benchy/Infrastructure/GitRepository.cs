using LibGit2Sharp;

namespace Benchy.Infrastructure;

public sealed class GitRepository : IDisposable
{
    private readonly Repository _repository;

    private GitRepository(Repository repository)
    {
        _repository = repository;
    }

    public static GitRepository Open(string path)
    {
        var repo = new Repository(path);
        return new GitRepository(repo);
    }

    public static GitRepository Clone(GitRepository sourceRepository, string targetPath)
    {
        var sourcePath = sourceRepository._repository.Info.Path;

        Repository.Clone(sourcePath, targetPath, new CloneOptions { Checkout = false });

        var repo = new Repository(targetPath);
        return new GitRepository(repo);
    }

    public GitRepository Checkout(string reference)
    {
        var commit = LoadAndValidateCommit(reference);
        Commands.Checkout(_repository, commit);

        return this;
    }

    public void Delete()
    {
        Directory.Delete(_repository.Info.WorkingDirectory, recursive: true);
    }

    private Commit LoadAndValidateCommit(string reference)
    {
        var commit =
            _repository.Lookup<Commit>(reference)
            ?? throw new ArgumentException($"Commit {reference} does not exist in the repository.");
        return commit;
    }

    void IDisposable.Dispose()
    {
        _repository.Dispose();
    }
}
