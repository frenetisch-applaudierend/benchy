using System.Text.Json;

namespace FakeBenchmarkDotNet;

public static class BenchmarkResultTemplates
{
    public static string CreateHashingBenchmarkResult(string title = "HashingBenchmark-20250720-144827", double meanMultiplier = 1.0)
    {
        var baseMean = 1456.23;
        var adjustedMean = baseMean * meanMultiplier;
        
        var result = new
        {
            Title = title,
            HostEnvironmentInfo = new
            {
                BenchmarkDotNetCaption = "BenchmarkDotNet",
                BenchmarkDotNetVersion = "0.14.0",
                OsVersion = "Linux (Fake)",
                ProcessorName = "Fake CPU for Testing",
                PhysicalProcessorCount = 1,
                PhysicalCoreCount = 4,
                LogicalCoreCount = 8,
                RuntimeVersion = ".NET 9.0",
                Architecture = "X64",
                HasAttachedDebugger = false,
                HasRyuJit = true,
                Configuration = "RELEASE",
                DotNetCliVersion = "9.0.302"
            },
            Benchmarks = new[]
            {
                new
                {
                    DisplayInfo = "HashingBenchmark.Hash: DefaultJob [N=1000]",
                    Namespace = (string?)null,
                    Type = "HashingBenchmark",
                    Method = "Hash",
                    MethodTitle = "Hash",
                    Parameters = "N=1000",
                    FullName = "HashingBenchmark.Hash(N: 1000)",
                    Statistics = new
                    {
                        OriginalValues = GenerateValues(adjustedMean, 13),
                        N = 13,
                        Min = adjustedMean * 0.994,
                        LowerFence = adjustedMean * 0.99,
                        Q1 = adjustedMean * 0.998,
                        Median = adjustedMean * 1.0,
                        Mean = adjustedMean,
                        Q3 = adjustedMean * 1.002,
                        UpperFence = adjustedMean * 1.01,
                        Max = adjustedMean * 1.005,
                        InterquartileRange = adjustedMean * 0.004,
                        StandardError = adjustedMean * 0.0009,
                        Variance = Math.Pow(adjustedMean * 0.003, 2),
                        StandardDeviation = adjustedMean * 0.003,
                        Skewness = 0.009,
                        Kurtosis = 1.77
                    },
                    Memory = new
                    {
                        Gen0Collections = 6,
                        Gen1Collections = 0,
                        Gen2Collections = 0,
                        TotalOperations = 524288,
                        BytesAllocatedPerOperation = 80
                    }
                },
                new
                {
                    DisplayInfo = "HashingBenchmark.Hash: DefaultJob [N=10000]",
                    Namespace = (string?)null,
                    Type = "HashingBenchmark",
                    Method = "Hash",
                    MethodTitle = "Hash",
                    Parameters = "N=10000",
                    FullName = "HashingBenchmark.Hash(N: 10000)",
                    Statistics = new
                    {
                        OriginalValues = GenerateValues(adjustedMean * 7.5, 15), // ~7.5x slower for 10x data
                        N = 15,
                        Min = adjustedMean * 7.46,
                        LowerFence = adjustedMean * 7.4,
                        Q1 = adjustedMean * 7.49,
                        Median = adjustedMean * 7.51,
                        Mean = adjustedMean * 7.5,
                        Q3 = adjustedMean * 7.52,
                        UpperFence = adjustedMean * 7.6,
                        Max = adjustedMean * 7.55,
                        InterquartileRange = adjustedMean * 0.03,
                        StandardError = adjustedMean * 0.008,
                        Variance = Math.Pow(adjustedMean * 0.03, 2),
                        StandardDeviation = adjustedMean * 0.03,
                        Skewness = 0.31,
                        Kurtosis = 1.58
                    },
                    Memory = new
                    {
                        Gen0Collections = 0,
                        Gen1Collections = 0,
                        Gen2Collections = 0,
                        TotalOperations = 65536,
                        BytesAllocatedPerOperation = 80
                    }
                }
            }
        };
        
        return JsonSerializer.Serialize(result, new JsonSerializerOptions 
        { 
            WriteIndented = true,
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        });
    }
    
    public static string CreateAlgorithmBenchmarkResult(string title = "AlgorithmBenchmark-20250720-144827", double meanMultiplier = 1.0)
    {
        var baseMean = 2345.67;
        var adjustedMean = baseMean * meanMultiplier;
        
        var result = new
        {
            Title = title,
            HostEnvironmentInfo = new
            {
                BenchmarkDotNetCaption = "BenchmarkDotNet",
                BenchmarkDotNetVersion = "0.14.0",
                OsVersion = "Linux (Fake)",
                ProcessorName = "Fake CPU for Testing",
                RuntimeVersion = ".NET 9.0",
                Architecture = "X64"
            },
            Benchmarks = new[]
            {
                new
                {
                    DisplayInfo = "AlgorithmBenchmark.Sort: DefaultJob",
                    Type = "AlgorithmBenchmark",
                    Method = "Sort",
                    FullName = "AlgorithmBenchmark.Sort()",
                    Statistics = new
                    {
                        N = 10,
                        Min = adjustedMean * 0.995,
                        Mean = adjustedMean,
                        Max = adjustedMean * 1.008,
                        StandardDeviation = adjustedMean * 0.002
                    },
                    Memory = new
                    {
                        Gen0Collections = 2,
                        Gen1Collections = 0,
                        Gen2Collections = 0,
                        TotalOperations = 100000,
                        BytesAllocatedPerOperation = 160
                    }
                }
            }
        };
        
        return JsonSerializer.Serialize(result, new JsonSerializerOptions 
        { 
            WriteIndented = true,
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        });
    }
    
    private static double[] GenerateValues(double mean, int count)
    {
        var random = new Random(42); // Deterministic for testing
        var values = new double[count];
        
        for (int i = 0; i < count; i++)
        {
            // Generate values with small variance around the mean
            var variance = mean * 0.003; // 0.3% variance
            values[i] = mean + (random.NextDouble() - 0.5) * variance * 2;
        }
        
        return values;
    }
}