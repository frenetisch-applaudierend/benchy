using System;

namespace Benchy.Infrastructure;

public static class Directories
{
    public static DirectoryInfo SubDirectory(this DirectoryInfo directory, string subDirectoryName)
    {
        return new DirectoryInfo(Path.Combine(directory.FullName, subDirectoryName));
    }

    public static FileInfo File(this DirectoryInfo directory, string fileName)
    {
        return new FileInfo(Path.Combine(directory.FullName, fileName));
    }
}

public sealed class TemporaryDirectory : IDisposable
{
    private readonly DirectoryInfo directory;
    private readonly bool keepAfterDisposal;

    public string FullName => directory.FullName;

    private TemporaryDirectory(DirectoryInfo directory, bool keepAfterDisposal)
    {
        this.directory = directory;
        this.keepAfterDisposal = keepAfterDisposal;

        if (keepAfterDisposal)
        {
            // No need to run the finalizer
            GC.SuppressFinalize(this);
        }
    }

    ~TemporaryDirectory()
    {
        // Finalizers are tricky. Keep the check, even if we should not reach the finalizer if the check fails.
        if (keepAfterDisposal)
        {
            return;
        }

        Delete();
    }

    public static TemporaryDirectory CreateNew(bool keep)
    {
        var now = DateTime.Now;

        var tempPath = Path.Combine(
            Path.GetTempPath(),
            "Benchy",
            $"{now:yyyy-MM-dd}",
            $"{now:HHmmss}_{Path.GetRandomFileName()}"
        );
        Directory.CreateDirectory(tempPath);
        return new TemporaryDirectory(new DirectoryInfo(tempPath), keepAfterDisposal: keep);
    }

    public DirectoryInfo CreateSubDirectory(string subDirectoryName)
    {
        return directory.CreateSubdirectory(subDirectoryName);
    }

    public void Delete()
    {
        try
        {
            directory.Delete(true);
        }
        catch
        {
            // ignore
        }
    }

    public override string ToString()
    {
        return directory.ToString();
    }

    void IDisposable.Dispose()
    {
        if (keepAfterDisposal)
        {
            return;
        }

        Delete();
        GC.SuppressFinalize(this);
    }
}
