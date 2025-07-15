using System;

namespace Benchy.Infrastructure;

public static class Directories
{
    public static DirectoryInfo CreateTemporaryDirectory(string reference)
    {
        var tempPath = Path.GetTempPath();
        var tempDirectory = Path.Combine(
            tempPath,
            "Benchy",
            $"{reference}@{Path.GetTempFileName()}"
        );
        Directory.CreateDirectory(tempDirectory);
        return new DirectoryInfo(tempDirectory);
    }

    public static DirectoryInfo SubDirectory(this DirectoryInfo directory, string subDirectoryName)
    {
        return new DirectoryInfo(Path.Combine(directory.FullName, subDirectoryName));
    }

    public static FileInfo File(this DirectoryInfo directory, string fileName)
    {
        return new FileInfo(Path.Combine(directory.FullName, fileName));
    }
}
