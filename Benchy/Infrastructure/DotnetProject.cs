using System.Diagnostics;

namespace Benchy.Infrastructure;

public class DotnetProject
{
    private readonly FileInfo projectFile;

    public string Name => projectFile.Name;

    public static DotnetProject Open(FileInfo projectFile)
    {
        foreach (var possibleLocation in GetPossibleProjectLocations(projectFile))
        {
            if (possibleLocation.Exists)
            {
                return new DotnetProject(possibleLocation);
            }
        }

        throw new FileNotFoundException($"Project file not found: {projectFile.FullName}");

        static IEnumerable<FileInfo> GetPossibleProjectLocations(FileInfo projectFile)
        {
            if (projectFile.Extension.Equals(".csproj", StringComparison.OrdinalIgnoreCase))
            {
                yield return projectFile;
                yield break;
            }

            yield return new FileInfo(projectFile.FullName + ".csproj");
            yield return new FileInfo(
                Path.Combine(projectFile.FullName, projectFile.Name + ".csproj")
            );
        }
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
