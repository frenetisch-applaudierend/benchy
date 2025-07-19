namespace Benchy.Infrastructure;

public static class Directories
{
    public static DirectoryInfo Subdirectory(this DirectoryInfo directory, string subDirectoryName)
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
    private bool keepAfterDisposal;

    public DirectoryInfo Directory { get; }

    public string FullName => Directory.FullName;

    private TemporaryDirectory(DirectoryInfo directory)
    {
        Directory = directory;
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

    public static TemporaryDirectory CreateNew()
    {
        var now = DateTime.Now;

        var tempPath = Path.Combine(
            Path.GetTempPath(),
            "Benchy",
            $"{now:yyyy-MM-dd}",
            $"{now:HHmmss}_{Path.GetRandomFileName()}"
        );
        System.IO.Directory.CreateDirectory(tempPath);
        return new TemporaryDirectory(new DirectoryInfo(tempPath));
    }

    public DirectoryInfo CreateSubdirectory(string subDirectoryName)
    {
        return Directory.CreateSubdirectory(subDirectoryName);
    }

    public void KeepAfterDisposal()
    {
        keepAfterDisposal = true;

        // No need to run the finalizer
        GC.SuppressFinalize(this);
    }

    public void Delete()
    {
        try
        {
            Directory.Delete(true);
        }
        catch
        {
            // ignore
        }
    }

    public override string ToString()
    {
        return Directory.ToString();
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
