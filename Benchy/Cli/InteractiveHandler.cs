namespace Benchy.Cli;

public static class InteractiveHandler
{
    public static void Handle(bool verbose, string[] benchmarks, DirectoryInfo? repositoryPath, bool noDelete, string[] commitRefs)
    {
        Console.WriteLine("=== INTERACTIVE MODE ===");
        Console.WriteLine($"Commit refs: [{string.Join(", ", commitRefs)}]");
        Console.WriteLine($"Repository path: {repositoryPath?.FullName ?? "auto-detect from current directory"}");
        Console.WriteLine($"Benchmarks: [{string.Join(", ", benchmarks)}]");
        Console.WriteLine($"No delete: {noDelete}");
        Console.WriteLine($"Verbose: {verbose}");
    }
}