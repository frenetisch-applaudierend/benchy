namespace Benchy.Infrastructure.Reporting;

public static class Reporting
{
    public static IReporter CreateReporter(
        string[] outputStyles,
        DirectoryInfo outputDirectory,
        TextWriter consoleWriter,
        bool useColors,
        bool isInteractiveMode
    )
    {
        var reporters = new List<IReporter>();

        // Apply defaults if no styles specified
        if (outputStyles.Length == 0)
        {
            outputStyles = isInteractiveMode ? ["console"] : ["json", "markdown"];
        }

        foreach (var style in outputStyles)
        {
            switch (style.ToLowerInvariant())
            {
                case "console":
                    reporters.Add(new OutputReporter(consoleWriter, useColors));
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
