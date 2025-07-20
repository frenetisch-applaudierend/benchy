using Benchy.Core;

namespace Benchy.Reporting;

public class CompositeReporter : IReporter
{
    private readonly IReadOnlyList<IReporter> reporters;

    public CompositeReporter(IEnumerable<IReporter> reporters)
    {
        this.reporters = [.. reporters];
    }

    public CompositeReporter(params IReporter[] reporters)
    {
        this.reporters = [.. reporters];
    }

    public void GenerateReport(BenchmarkComparisonResult result)
    {
        foreach (var reporter in reporters)
        {
            try
            {
                reporter.GenerateReport(result);
            }
            catch (Exception ex)
            {
                // Log the error but continue with other reporters
                Console.Error.WriteLine(
                    $"Error generating report with {reporter.GetType().Name}: {ex.Message}"
                );
            }
        }
    }
}
