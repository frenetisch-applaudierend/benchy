using Benchy.Core;
using Benchy.Output;
using static Benchy.Output.FormattedText;

namespace Benchy.Reporting;

public class OutputReporter : IReporter
{
    private static readonly FormattedText Arrow = Decor("▷▷ ", "=>");
    private static readonly FormattedText ImprovementLower = Decor(
        Colored("▼", ConsoleColor.Green),
        "[IMP]"
    );
    private static readonly FormattedText ImprovementHigher = Decor(
        Colored("▲", ConsoleColor.Green),
        "[IMP]"
    );
    private static readonly FormattedText ImprovementAny = Decor(
        Colored("▼ ▲", ConsoleColor.Green),
        "[IMP]"
    );
    private static readonly FormattedText RegressionLower = Decor(
        Colored("▼", ConsoleColor.Red),
        "[REG]"
    );
    private static readonly FormattedText RegressionHigher = Decor(
        Colored("▲", ConsoleColor.Red),
        "[REG]"
    );
    private static readonly FormattedText RegressionAny = Decor(
        Colored("▼ ▲", ConsoleColor.Red),
        "[REG]"
    );
    private static readonly FormattedText Stable = Decor(
        Colored("≈", ConsoleColor.DarkGreen),
        "[STA]"
    );
    private static readonly FormattedText Irrelevant = Decor(" ", "     ");
    private static readonly FormattedText Uncompared = Decor("○", "     ");

    public void GenerateReport(BenchmarkComparisonResult result)
    {
        CliOutput.Info("Benchmark comparison report");

        foreach (var comparison in result.Comparisons)
        {
            PrintComparsion(comparison, result.SignificanceThreshold);
        }

        PrintSummary(result);
    }

    private static void PrintSummary(BenchmarkComparisonResult result)
    {
        var improvements = result.Comparisons.Count(c =>
            c.Statistics.Mean.HasSignificantChange(result.SignificanceThreshold * 100)
            && c.Statistics.Mean.Delta < 0.0
        );
        var regressions = result.Comparisons.Count(c =>
            c.Statistics.Mean.HasSignificantChange(result.SignificanceThreshold * 100)
            && c.Statistics.Mean.Delta > 0.0
        );

        if (improvements == 0 && regressions == 0)
        {
            CliOutput.Info($"{Decor("📊 ")}No significant changes detected.");
        }
        else
        {
            CliOutput.Info($"{Decor("📊 ")}Significant changes detected:");
            if (improvements > 0)
                CliOutput.Info($"{ImprovementAny} improvements: {improvements}", indent: 2);
            if (regressions > 0)
                CliOutput.Info($"{RegressionAny} regressions: {regressions}", indent: 2);
        }
    }

    private static void PrintComparsion(
        BenchmarkComparison comparison,
        double significanceThreshold
    )
    {
        CliOutput.Info($"{Decor("🏁 ")}Results for {Em(comparison.FullName)}", indent: 1);

        PrintDuration(
            name: "Mean",
            value: comparison.Statistics.Mean,
            comparisonType: ComparisonType.LowerIsBetter,
            significanceThreshold: significanceThreshold
        );
        PrintDuration(
            name: "Error",
            value: comparison.Statistics.StandardError,
            comparisonType: ComparisonType.Irrelevant,
            significanceThreshold: significanceThreshold
        );
        PrintDuration(
            name: "StdDev",
            value: comparison.Statistics.StandardDeviation,
            comparisonType: ComparisonType.Irrelevant,
            significanceThreshold: significanceThreshold
        );
    }

    private static void PrintDuration(
        string name,
        ComparisonValue<double> value,
        ComparisonType comparisonType,
        double significanceThreshold
    )
    {
        var (baseline, target) = FormatDuration(value);

        var resultSymbol = GetResultSymbol(value, comparisonType, significanceThreshold);

        var percentageChange =
            comparisonType != ComparisonType.Irrelevant
            && value.PercentageChange is { } percentageChangeValue
                ? $" ({percentageChangeValue:F1} %)"
                : "";

        CliOutput.Info(
            $"{resultSymbol} {Em(name)}: {baseline} {Arrow} {target}{percentageChange}",
            indent: 2
        );
    }

    private static FormattedText GetResultSymbol(
        ComparisonValue<double> value,
        ComparisonType comparisonType,
        double significanceThreshold
    )
    {
        if (comparisonType == ComparisonType.Irrelevant)
            return Irrelevant;

        if (value.Baseline == null || value.Target == null)
            return Uncompared;

        if (!value.HasSignificantChange(significanceThreshold * 100))
            return Stable;

        if (value.Delta < 0.0)
        {
            return comparisonType == ComparisonType.LowerIsBetter
                ? ImprovementLower
                : RegressionLower;
        }
        else
        {
            return comparisonType == ComparisonType.HigherIsBetter
                ? ImprovementHigher
                : RegressionHigher;
        }
    }

    private static (string, string) FormatDuration(ComparisonValue<double> value)
    {
        if (value.Baseline == null && value.Target == null)
        {
            return ("n/a", "n/a");
        }

        var scaledBaseline = value.Baseline;
        var scaledTarget = value.Target;

        string[] units = ["ns", "μs", "ms", "s"];
        var unitIndex = 0;

        while (SmallerOf(scaledBaseline, scaledTarget) > 1000.0 && unitIndex < units.Length - 1)
        {
            scaledBaseline /= 1000;
            scaledTarget /= 1000;
            unitIndex++;
        }

        var unit = units[unitIndex];
        var baseline = Format(scaledBaseline, unit);
        var target = Format(scaledTarget, unit);
        return (baseline, target);

        static double SmallerOf(double? a, double? b)
        {
            if (a == null || a == 0.0)
                return b!.Value;
            if (b == null || b == 0.0)
                return a!.Value;

            return Math.Min(a.Value, b.Value);
        }

        static string Format(double? value, string unit)
        {
            if (value == null)
                return "n/a";

            return $"{value:F2} {unit}";
        }
    }

    private enum ComparisonType
    {
        Irrelevant,
        LowerIsBetter,
        HigherIsBetter,
    }
}
