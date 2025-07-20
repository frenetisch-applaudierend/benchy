using System.Text.Json;
using Benchy.Core;

namespace Benchy.Tests.Unit.Core;

public class BenchmarkReportTests
{
    [Fact]
    public void LoadReport_ValidJsonFile_ReturnsBenchmarkReport()
    {
        // Arrange
        var testData = CreateSampleBenchmarkReport();
        var json = JsonSerializer.Serialize(testData);
        var tempFile = CreateTempJsonFile(json);

        try
        {
            // Act
            var result = BenchmarkReport.LoadReport(tempFile);

            // Assert
            result.Should().NotBeNull();
            result.Title.Should().Be("HashingBenchmark-20250720-144827");
            result.Benchmarks.Should().HaveCount(2);

            var firstBenchmark = result.Benchmarks.First();
            firstBenchmark.FullName.Should().Be("HashingBenchmark.Hash(N: 1000)");
            firstBenchmark.Statistics.Mean.Should().Be(1456.23);
        }
        finally
        {
            if (tempFile.Exists)
                tempFile.Delete();
        }
    }

    [Fact]
    public void LoadReport_InvalidJsonFile_ThrowsJsonException()
    {
        // Arrange
        var invalidJson = "{ invalid json }";
        var tempFile = CreateTempJsonFile(invalidJson);

        try
        {
            // Act & Assert
            var act = () => BenchmarkReport.LoadReport(tempFile);
            act.Should().Throw<JsonException>();
        }
        finally
        {
            if (tempFile.Exists)
                tempFile.Delete();
        }
    }

    [Fact]
    public void LoadReport_EmptyJsonFile_ThrowsInvalidOperationException()
    {
        // Arrange
        var tempFile = CreateTempJsonFile("null");

        try
        {
            // Act & Assert
            var act = () => BenchmarkReport.LoadReport(tempFile);
            act.Should()
                .Throw<InvalidOperationException>()
                .WithMessage("Failed to deserialize report");
        }
        finally
        {
            if (tempFile.Exists)
                tempFile.Delete();
        }
    }

    [Fact]
    public void LoadReports_DirectoryWithMultipleReports_ReturnsAllReports()
    {
        // Arrange
        var tempDir = CreateTempDirectory();

        try
        {
            var report1 = CreateSampleBenchmarkReport("Report1");
            var report2 = CreateSampleBenchmarkReport("Report2");

            CreateReportFileInDirectory(tempDir, "Report1-report-full-compressed.json", report1);
            CreateReportFileInDirectory(tempDir, "Report2-report-full-compressed.json", report2);
            CreateReportFileInDirectory(tempDir, "other-file.json", report1); // Should be ignored

            // Act
            var results = BenchmarkReport.LoadReports(tempDir).ToList();

            // Assert
            results.Should().HaveCount(2);
            results.Should().Contain(r => r.Title == "Report1");
            results.Should().Contain(r => r.Title == "Report2");
        }
        finally
        {
            if (tempDir.Exists)
                tempDir.Delete(recursive: true);
        }
    }

    [Fact]
    public void LoadReports_EmptyDirectory_ReturnsEmptyCollection()
    {
        // Arrange
        var tempDir = CreateTempDirectory();

        try
        {
            // Act
            var results = BenchmarkReport.LoadReports(tempDir);

            // Assert
            results.Should().BeEmpty();
        }
        finally
        {
            if (tempDir.Exists)
                tempDir.Delete(recursive: true);
        }
    }

    [Theory]
    [InlineData("HashingBenchmark-report-full-compressed.json")]
    [InlineData("AlgorithmBenchmark-report-full-compressed.json")]
    [InlineData("prefix-report-full-compressed.json")]
    public void LoadReports_FileMatchingPattern_IsIncluded(string fileName)
    {
        // Arrange
        var tempDir = CreateTempDirectory();

        try
        {
            var report = CreateSampleBenchmarkReport();
            CreateReportFileInDirectory(tempDir, fileName, report);

            // Act
            var results = BenchmarkReport.LoadReports(tempDir).ToList();

            // Assert
            results.Should().HaveCount(1);
        }
        finally
        {
            if (tempDir.Exists)
                tempDir.Delete(recursive: true);
        }
    }

    [Theory]
    [InlineData("report-full.json")]
    [InlineData("report-compressed.json")]
    [InlineData("other-report.json")]
    [InlineData("benchmark-results.json")]
    public void LoadReports_FileNotMatchingPattern_IsExcluded(string fileName)
    {
        // Arrange
        var tempDir = CreateTempDirectory();

        try
        {
            var report = CreateSampleBenchmarkReport();
            CreateReportFileInDirectory(tempDir, fileName, report);

            // Act
            var results = BenchmarkReport.LoadReports(tempDir);

            // Assert
            results.Should().BeEmpty();
        }
        finally
        {
            if (tempDir.Exists)
                tempDir.Delete(recursive: true);
        }
    }

    private static BenchmarkReport CreateSampleBenchmarkReport(
        string title = "HashingBenchmark-20250720-144827"
    )
    {
        var statistics = new BenchmarkReport.Statistics(
            Mean: 1456.23,
            Min: 1448.04,
            Max: 1463.55,
            Median: 1455.68,
            StandardDeviation: 4.66,
            StandardError: 1.29,
            Variance: 21.67,
            Skewness: 0.009,
            Kurtosis: 1.77,
            ConfidenceInterval: new BenchmarkReport.ConfidenceInterval(
                N: 13,
                Mean: 1456.23,
                StandardError: 1.29,
                Level: 12,
                Margin: 5.58,
                Lower: 1450.66,
                Upper: 1461.81
            ),
            Percentiles: new BenchmarkReport.Percentiles(
                P0: 1448.04,
                P25: 1453.20,
                P50: 1455.68,
                P67: 1457.98,
                P80: 1460.08,
                P85: 1460.69,
                P90: 1462.34,
                P95: 1463.15,
                P100: 1463.55
            )
        );

        var memory = new BenchmarkReport.MemoryMetrics(
            BytesAllocatedPerOperation: 80,
            Gen0Collections: 6,
            Gen1Collections: 0,
            Gen2Collections: 0,
            TotalOperations: 524288
        );

        var benchmarks = new[]
        {
            new BenchmarkReport.Benchmark("HashingBenchmark.Hash(N: 1000)", statistics, memory),
            new BenchmarkReport.Benchmark("HashingBenchmark.Hash(N: 10000)", statistics, memory),
        };

        return new BenchmarkReport(title, benchmarks);
    }

    private static FileInfo CreateTempJsonFile(string content)
    {
        var tempPath = Path.GetTempFileName();
        File.WriteAllText(tempPath, content);
        return new FileInfo(tempPath);
    }

    private static DirectoryInfo CreateTempDirectory()
    {
        var tempPath = Path.Combine(Path.GetTempPath(), $"benchy-test-{Guid.NewGuid():N}");
        return Directory.CreateDirectory(tempPath);
    }

    private static void CreateReportFileInDirectory(
        DirectoryInfo directory,
        string fileName,
        BenchmarkReport report
    )
    {
        var json = JsonSerializer.Serialize(report);
        var filePath = Path.Combine(directory.FullName, fileName);
        File.WriteAllText(filePath, json);
    }
}
