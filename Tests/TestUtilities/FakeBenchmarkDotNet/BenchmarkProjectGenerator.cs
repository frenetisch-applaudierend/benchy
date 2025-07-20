using System.Text.Json;

namespace FakeBenchmarkDotNet;

public class BenchmarkProjectGenerator
{
    public static GeneratedBenchmarkProject CreateProject(
        string projectName,
        FakeBenchmarkConfig config,
        DirectoryInfo parentDirectory)
    {
        var projectDir = parentDirectory.CreateSubdirectory(projectName);
        
        // Create the project file
        var csprojContent = CreateProjectFileContent();
        var csprojPath = Path.Combine(projectDir.FullName, $"{projectName}.csproj");
        File.WriteAllText(csprojPath, csprojContent);
        
        // Create Program.cs that uses the fake benchmark runner
        var programContent = CreateProgramContent(config);
        var programPath = Path.Combine(projectDir.FullName, "Program.cs");
        File.WriteAllText(programPath, programContent);
        
        return new GeneratedBenchmarkProject(projectDir, projectName, config);
    }
    
    private static string CreateProjectFileContent()
    {
        return """
            <Project Sdk="Microsoft.NET.Sdk">
              <PropertyGroup>
                <OutputType>Exe</OutputType>
                <TargetFramework>net9.0</TargetFramework>
                <ImplicitUsings>enable</ImplicitUsings>
                <Nullable>enable</Nullable>
              </PropertyGroup>
              
              <ItemGroup>
                <ProjectReference Include="../../../TestUtilities/FakeBenchmarkDotNet/FakeBenchmarkDotNet.csproj" />
              </ItemGroup>
            </Project>
            """;
    }
    
    private static string CreateProgramContent(FakeBenchmarkConfig config)
    {
        var configJson = JsonSerializer.Serialize(config, new JsonSerializerOptions 
        { 
            WriteIndented = true,
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        });
        
        return $"""
            using FakeBenchmarkDotNet;
            using System.Text.Json;
            
            public static class Program
            {{
                public static async Task<int> Main(string[] args)
                {{
                    var configJson = """
                    {configJson}
                    """;
                    
                    var config = JsonSerializer.Deserialize<FakeBenchmarkConfig>(configJson, new JsonSerializerOptions
                    {{
                        PropertyNamingPolicy = JsonNamingPolicy.CamelCase
                    }});
                    
                    return await FakeBenchmarkRunner.RunAsync(args, config!);
                }}
            }}
            """;
    }
}

public class GeneratedBenchmarkProject : IDisposable
{
    public DirectoryInfo ProjectDirectory { get; }
    public string ProjectName { get; }
    public FakeBenchmarkConfig Config { get; }
    
    public GeneratedBenchmarkProject(DirectoryInfo projectDirectory, string projectName, FakeBenchmarkConfig config)
    {
        ProjectDirectory = projectDirectory;
        ProjectName = projectName;
        Config = config;
    }
    
    public string ProjectPath => Path.Combine(ProjectDirectory.FullName, $"{ProjectName}.csproj");
    
    public void Dispose()
    {
        if (ProjectDirectory.Exists)
        {
            ProjectDirectory.Delete(recursive: true);
        }
    }
}