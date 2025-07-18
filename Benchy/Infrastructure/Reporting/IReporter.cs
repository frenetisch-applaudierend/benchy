using Benchy.Core;

namespace Benchy.Infrastructure.Reporting;

public interface IReporter
{
    void GenerateReport(BenchmarkComparisonResult result);
}
