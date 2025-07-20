using Benchy.Core;

namespace Benchy.Reporting;

public interface IReporter
{
    void GenerateReport(BenchmarkComparisonResult result);
}
