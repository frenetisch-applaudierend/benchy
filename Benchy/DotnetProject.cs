using System.Diagnostics;

namespace Benchy;

public class DotnetProject
{
    private readonly FileInfo projectFile;

    public static DotnetProject Open(string path)
    {
        var projectFile = new FileInfo(path);

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

    public void Build()
    {
        ExecuteCommand("build", $"\"{projectFile.FullName}\"", "Build failed");
    }

    public void Run(IEnumerable<string> args)
    {
        ExecuteCommand("run", $"--project \"{projectFile.FullName}\" -- {string.Join(' ', args)}", "Run failed");
    }

    private void ExecuteCommand(string command, string arguments, string errorMessage)
    {
        var process = new Process
        {
            StartInfo = new ProcessStartInfo
            {
                FileName = "dotnet",
                Arguments = $"{command} {arguments}",
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                UseShellExecute = false,
                CreateNoWindow = true
            }
        };

        process.Start();
        process.WaitForExit();

        if (process.ExitCode != 0)
        {
            throw new Exception($"{errorMessage}: {process.StandardError.ReadToEnd()}");
        }
    }
}
