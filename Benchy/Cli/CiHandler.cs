namespace Benchy.Cli;

public static class CiHandler
{
    public static void Handle(bool verbose, string[] benchmarks, DirectoryInfo baselineDirectory, DirectoryInfo targetDirectory)
    {
        Console.WriteLine("=== CI MODE ===");
        Console.WriteLine($"Baseline directory: {baselineDirectory.FullName}");
        Console.WriteLine($"Target directory: {targetDirectory.FullName}");
        Console.WriteLine($"Benchmarks: [{string.Join(", ", benchmarks)}]");
        Console.WriteLine($"Verbose: {verbose}");
    }
}