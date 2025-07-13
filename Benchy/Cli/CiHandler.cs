namespace Benchy.Cli;

public static class CiHandler
{
    public static void Handle(bool verbose, string[] benchmarks, DirectoryInfo[] directories)
    {
        Console.WriteLine("=== CI MODE ===");
        Console.WriteLine($"Directories: [{string.Join(", ", directories.Select(d => d.FullName))}]");
        Console.WriteLine($"Benchmarks: [{string.Join(", ", benchmarks)}]");
        Console.WriteLine($"Verbose: {verbose}");
    }
}