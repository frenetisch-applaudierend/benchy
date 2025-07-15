using System.Diagnostics;

namespace Benchy.Infrastructure;

public class DotnetProject
{
    private readonly FileInfo projectFile;

    public string Name => projectFile.Name;

    public static DotnetProject Open(FileInfo projectFile)
    {
        if (!projectFile.Extension.Equals(".csproj", StringComparison.OrdinalIgnoreCase))
        {
            projectFile = new FileInfo(projectFile.FullName + ".csproj");
        }

        if (!projectFile.Exists)
        {
            throw new FileNotFoundException($"Project file not found: {projectFile.FullName}");
        }

        return new DotnetProject(projectFile);
    }

    private DotnetProject(FileInfo projectFile)
    {
        this.projectFile = projectFile;
    }

    public void Build(bool verbose)
    {
        ExecuteCommand(
            "build",
            $"\"{projectFile.FullName}\" --configuration Release",
            "Build failed",
            verbose
        );
    }

    public void Run(IEnumerable<string> args, bool verbose)
    {
        ExecuteCommand(
            "run",
            $"--project \"{projectFile.FullName}\" --no-build --configuration Release -- {string.Join(' ', args)}",
            "Run failed",
            verbose
        );
    }

    private void ExecuteCommand(string command, string arguments, string errorMessage, bool verbose)
    {
        var process = new Process
        {
            StartInfo = new ProcessStartInfo
            {
                FileName = "dotnet",
                Arguments = $"{command} {arguments}",
                RedirectStandardOutput = !verbose,
                RedirectStandardError = !verbose,
                UseShellExecute = false,
                CreateNoWindow = true,
                WorkingDirectory = projectFile.DirectoryName,
            },
        };

        process.Start();
        process.WaitForExit();

        if (process.ExitCode != 0)
        {
            throw new Exception(errorMessage);
        }
    }
}
