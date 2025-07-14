namespace Benchy.Cli;

public static class InteractiveHandler
{
    public static void Handle(
        bool verbose,
        string[] benchmarks,
        DirectoryInfo? repositoryPath,
        bool noDelete,
        string baselineRef,
        string? targetRef
    )
    {
        // Default to currently checked out version if target not specified
        var effectiveTargetRef = targetRef ?? "HEAD";

        Console.WriteLine("=== INTERACTIVE MODE ===");
        Console.WriteLine($"Baseline ref: {baselineRef}");
        Console.WriteLine(
            $"Target ref: {effectiveTargetRef}{(targetRef == null ? " (currently checked out version)" : "")}"
        );
        Console.WriteLine(
            $"Repository path: {repositoryPath?.FullName ?? "auto-detect from current directory"}"
        );
        Console.WriteLine($"Benchmarks: [{string.Join(", ", benchmarks)}]");
        Console.WriteLine($"No delete: {noDelete}");
        Console.WriteLine($"Verbose: {verbose}");
    }
}
