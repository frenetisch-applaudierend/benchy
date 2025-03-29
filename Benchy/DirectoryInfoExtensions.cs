using System;

namespace Benchy;

public static class DirectoryInfoExtensions
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
