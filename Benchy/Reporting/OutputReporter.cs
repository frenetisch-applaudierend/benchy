using System.Globalization;
using Benchy.Core;
using Benchy.Output;
using static Benchy.Output.FormattedText;

namespace Benchy.Reporting;

public class OutputReporter : IReporter
{
    public void GenerateReport(BenchmarkComparisonResult result)
    {
        CliOutput.Info("Benchmark comparison report");

        foreach (var comparison in result.Comparisons)
        {
            PrintComparsion(comparison, result.SignificanceThreshold);
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
            value.PercentageChange?.ToString("F1", CultureInfo.InvariantCulture) ?? "n/a";

        CliOutput.Info(
            $"{Decor($"{resultSymbol} ")}{Em(name)}: {baseline} → {target} ({percentageChange} %)",
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
        {
            return " ";
        }

        if (value.Baseline == null || value.Target == null)
        {
            return "○";
        }

        if (Math.Abs(value.Delta ?? 0) < double.Epsilon)
        {
            return "=";
        }

        var isSignificant = value.HasSignificantChange(significanceThreshold * 100);
        var isImprovement =
            comparisonType == ComparisonType.LowerIsBetter ? value.Delta < 0 : value.Delta > 0;

        if (value.Delta < 0)
        {
            // Downward arrow
            if (isSignificant)
            {
                // Colored solid arrow for significant changes
                return Colored(
                    "▼",
                    comparisonType == ComparisonType.LowerIsBetter
                        ? ConsoleColor.Green
                        : ConsoleColor.Red
                );
            }
            else
            {
                // Outlined arrow for non-significant changes
                return "▽"; // Using outlined triangle pointing down
            }
        }
        else
        {
            // Upward arrow
            if (isSignificant)
            {
                // Colored solid arrow for significant changes
                return Colored(
                    "▲",
                    comparisonType == ComparisonType.HigherIsBetter
                        ? ConsoleColor.Green
                        : ConsoleColor.Red
                );
            }
            else
            {
                // Outlined arrow for non-significant changes
                return "△"; // Using outlined triangle pointing up
            }
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
