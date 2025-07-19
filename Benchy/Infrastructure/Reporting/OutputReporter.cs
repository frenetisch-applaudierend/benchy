using System.Globalization;
using Benchy.Core;
using Benchy.Output;
using static Benchy.Output.FormattedText;

namespace Benchy.Infrastructure.Reporting;

public class OutputReporter : IReporter
{
    public void GenerateReport(BenchmarkComparisonResult result)
    {
        CliOutput.Info("Benchmark comparison report");

        foreach (var comparison in result.Comparisons)
        {
            PrintComparsion(comparison);
        }
    }

    private static void PrintComparsion(BenchmarkComparison comparison)
    {
        CliOutput.Info($"{Decor("🏁 ")}Results for {Em(comparison.FullName)}", indent: 1);

        PrintDuration(
            name: "Mean",
            value: comparison.Statistics.Mean,
            comparisonType: ComparisonType.LowerIsBetter
        );
        PrintDuration(
            name: "Error",
            value: comparison.Statistics.StandardError,
            comparisonType: ComparisonType.Irrelevant
        );
        PrintDuration(
            name: "StdDev",
            value: comparison.Statistics.StandardDeviation,
            comparisonType: ComparisonType.Irrelevant
        );
    }

    private static void PrintDuration(
        string name,
        ComparisonValue<double> value,
        ComparisonType comparisonType
    )
    {
        var (baseline, target) = FormatDuration(value);

        var resultSymbol = GetResultSymbol(value, comparisonType);

        var delta = value.Delta?.ToString("F2", CultureInfo.InvariantCulture) ?? "n/a";
        var percentageChange =
            value.PercentageChange?.ToString("F1", CultureInfo.InvariantCulture) ?? "n/a";

        CliOutput.Info(
            $"{Decor($"{resultSymbol} ")}{Em(name)}: {baseline} → {target} ({delta}, {percentageChange}%)",
            indent: 2
        );
    }

    private static FormattedText GetResultSymbol(
        ComparisonValue<double> value,
        ComparisonType comparisonType
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

        return value.Delta < 0
            ? Colored(
                "▼",
                comparisonType == ComparisonType.LowerIsBetter
                    ? ConsoleColor.Green
                    : ConsoleColor.Red
            )
            : Colored(
                "▲",
                comparisonType == ComparisonType.HigherIsBetter
                    ? ConsoleColor.Green
                    : ConsoleColor.Red
            );
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
        var baseline = scaledBaseline?.ToString("F2", CultureInfo.InvariantCulture) ?? "n/a";
        var target = scaledTarget?.ToString("F2", CultureInfo.InvariantCulture) ?? "n/a";
        return (baseline, target);

        static double SmallerOf(double? a, double? b)
        {
            if (a == null || a == 0.0)
                return b!.Value;
            if (b == null || b == 0.0)
                return a!.Value;

            return Math.Min(a.Value, b.Value);
        }
    }

    private enum ComparisonType
    {
        Irrelevant,
        LowerIsBetter,
        HigherIsBetter,
    }
}
