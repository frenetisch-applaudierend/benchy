using System.CommandLine;
using System.Text.Json;

namespace FakeBenchmarkDotNet;

public class FakeBenchmarkRunner
{
    public static async Task<int> RunAsync(string[] args, FakeBenchmarkConfig config)
    {
        var rootCommand = new RootCommand("Fake BenchmarkDotNet Runner");
        
        // Add all BenchmarkDotNet options that Benchy uses
        var keepFilesOption = new Option<bool>("--keepFiles", "Keep benchmark artifacts");
        var stopOnFirstErrorOption = new Option<bool>("--stopOnFirstError", "Stop on first error");
        var memoryOption = new Option<bool>("--memory", "Include memory allocation metrics");
        var threadingOption = new Option<bool>("--threading", "Include threading information");
        var exportersOption = new Option<string[]>("--exporters", "Output format exporters");
        var artifactsOption = new Option<DirectoryInfo>("--artifacts", "Artifacts output directory");
        
        rootCommand.AddOptions(keepFilesOption, stopOnFirstErrorOption, memoryOption, 
                             threadingOption, exportersOption, artifactsOption);
        
        rootCommand.SetHandler(async (keepFiles, stopOnFirstError, memory, threading, exporters, artifacts) =>
        {
            await ExecuteFakeBenchmark(config, keepFiles, stopOnFirstError, memory, threading, exporters, artifacts);
        }, keepFilesOption, stopOnFirstErrorOption, memoryOption, threadingOption, exportersOption, artifactsOption);
        
        return await rootCommand.InvokeAsync(args);
    }
    
    private static async Task ExecuteFakeBenchmark(
        FakeBenchmarkConfig config,
        bool keepFiles, 
        bool stopOnFirstError, 
        bool memory, 
        bool threading, 
        string[] exporters, 
        DirectoryInfo? artifacts)
    {
        try
        {
            if (config.ShouldFail)
            {
                Console.Error.WriteLine("Fake benchmark configured to fail");
                Environment.Exit(1);
                return;
            }
            
            // Simulate execution delay if configured
            if (config.ExecutionDelay > TimeSpan.Zero)
            {
                await Task.Delay(config.ExecutionDelay);
            }
            
            // Determine output directory
            var outputDir = artifacts?.FullName ?? "BenchmarkDotNet.Artifacts";
            Directory.CreateDirectory(outputDir);
            
            // Copy or generate the configured result files
            await GenerateOutputFiles(config, outputDir);
            
            Console.WriteLine("// ** Fake benchmark completed successfully **");
            Console.WriteLine($"// Generated output in: {outputDir}");
        }
        catch (Exception ex)
        {
            Console.Error.WriteLine($"Fake benchmark failed: {ex.Message}");
            Environment.Exit(1);
        }
    }
    
    private static async Task GenerateOutputFiles(FakeBenchmarkConfig config, string outputDir)
    {
        foreach (var outputFile in config.OutputFiles)
        {
            var targetPath = Path.Combine(outputDir, outputFile.FileName);
            
            if (File.Exists(outputFile.SourcePath))
            {
                // Copy pre-generated file
                File.Copy(outputFile.SourcePath, targetPath, overwrite: true);
            }
            else if (!string.IsNullOrEmpty(outputFile.JsonContent))
            {
                // Write JSON content directly
                await File.WriteAllTextAsync(targetPath, outputFile.JsonContent);
            }
            else
            {
                throw new InvalidOperationException($"No content specified for output file: {outputFile.FileName}");
            }
            
            Console.WriteLine($"// Generated: {targetPath}");
        }
    }
}