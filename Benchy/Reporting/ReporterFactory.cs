namespace Benchy.Reporting;

public static class ReporterFactory
{
    public static IReporter CreateReporter(string[] outputStyles, DirectoryInfo outputDirectory)
    {
        ArgumentOutOfRangeException.ThrowIfZero(outputStyles.Length);

        var reporters = new List<IReporter>();

        foreach (var style in outputStyles)
        {
            switch (style.ToLowerInvariant())
            {
                case "console":
                    reporters.Add(new OutputReporter());
                    break;

                case "json":
                    var jsonPath = Path.Combine(outputDirectory.FullName, "benchy-results.json");
                    reporters.Add(new JsonReporter(jsonPath));
                    break;

                case "markdown":
                    var markdownPath = Path.Combine(outputDirectory.FullName, "benchy-summary.md");
                    reporters.Add(new MarkdownReporter(markdownPath));
                    break;

                default:
                    throw new ArgumentException($"Unknown output style: {style}");
            }
        }

        return reporters.Count == 1 ? reporters[0] : new CompositeReporter(reporters);
    }
}
