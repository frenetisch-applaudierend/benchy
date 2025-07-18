using LibGit2Sharp;

namespace Benchy.Infrastructure;

public sealed class GitRepository : IDisposable
{
    private readonly Repository repository;

    private GitRepository(Repository repository)
    {
        this.repository = repository;
    }

    public static GitRepository Open(string path)
    {
        var repo = new Repository(path);
        return new GitRepository(repo);
    }

    public static GitRepository Clone(GitRepository sourceRepository, string targetPath)
    {
        var sourcePath = sourceRepository.repository.Info.Path;

        Repository.Clone(sourcePath, targetPath, new CloneOptions { Checkout = false });

        var repo = new Repository(targetPath);
        return new GitRepository(repo);
    }

    public DirectoryInfo? WorkingDirectory =>
        repository.Info.WorkingDirectory != null
            ? new DirectoryInfo(repository.Info.WorkingDirectory)
            : null;

    public GitRepository Checkout(string reference)
    {
        var commit = LoadAndValidateCommit(reference);
        Commands.Checkout(repository, commit);

        return this;
    }

    public void Delete()
    {
        Directory.Delete(repository.Info.WorkingDirectory, recursive: true);
    }

    private Commit LoadAndValidateCommit(string reference)
    {
        var commit =
            repository.Lookup<Commit>(reference)
            ?? throw new ArgumentException($"Commit {reference} does not exist in the repository.");
        return commit;
    }

    void IDisposable.Dispose()
    {
        repository.Dispose();
    }
}
