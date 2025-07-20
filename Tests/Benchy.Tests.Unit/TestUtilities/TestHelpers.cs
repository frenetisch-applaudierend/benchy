using System.Text.Json;

namespace Benchy.Tests.Unit.TestUtilities;

public static class TestHelpers
{
    public static FileInfo CreateTempFile(string content, string extension = ".txt")
    {
        var tempPath = Path.GetTempFileName();
        if (!string.IsNullOrEmpty(extension) && !tempPath.EndsWith(extension))
        {
            var newPath = Path.ChangeExtension(tempPath, extension);
            File.Move(tempPath, newPath);
            tempPath = newPath;
        }

        File.WriteAllText(tempPath, content);
        return new FileInfo(tempPath);
    }

    public static FileInfo CreateTempJsonFile<T>(T data)
    {
        var json = JsonSerializer.Serialize(data, JsonSerializerOptions);
        return CreateTempFile(json, ".json");
    }

    public static DirectoryInfo CreateTempDirectory()
    {
        var tempPath = Path.Combine(Path.GetTempPath(), $"benchy-test-{Guid.NewGuid():N}");
        return Directory.CreateDirectory(tempPath);
    }

    public static void CleanupFile(FileInfo file)
    {
        try
        {
            if (file.Exists)
                file.Delete();
        }
        catch
        {
            // Ignore cleanup errors
        }
    }

    public static void CleanupDirectory(DirectoryInfo directory)
    {
        try
        {
            if (directory.Exists)
                directory.Delete(recursive: true);
        }
        catch
        {
            // Ignore cleanup errors
        }
    }

    public static JsonSerializerOptions JsonSerializerOptions =>
        new() { PropertyNamingPolicy = JsonNamingPolicy.CamelCase, WriteIndented = true };
}

public static class StringWriterExtensions
{
    public static string GetOutputAndReset(this StringWriter writer)
    {
        var output = writer.ToString();
        writer.GetStringBuilder().Clear();
        return output;
    }
}

public class DisposableFile : IDisposable
{
    public FileInfo File { get; }

    public DisposableFile(string content, string extension = ".txt")
    {
        File = TestHelpers.CreateTempFile(content, extension);
    }

    public static DisposableFile CreateJson<T>(T data)
    {
        var json = JsonSerializer.Serialize(data, TestHelpers.JsonSerializerOptions);
        return new DisposableFile(json, ".json");
    }

    public void Dispose()
    {
        TestHelpers.CleanupFile(File);
    }
}

public class DisposableDirectory : IDisposable
{
    public DirectoryInfo Directory { get; }

    public DisposableDirectory()
    {
        Directory = TestHelpers.CreateTempDirectory();
    }

    public FileInfo CreateFile(string fileName, string content)
    {
        var filePath = Path.Combine(Directory.FullName, fileName);
        File.WriteAllText(filePath, content);
        return new FileInfo(filePath);
    }

    public DirectoryInfo CreateSubdirectory(string name)
    {
        var subdirPath = Path.Combine(Directory.FullName, name);
        return System.IO.Directory.CreateDirectory(subdirPath);
    }

    public void Dispose()
    {
        TestHelpers.CleanupDirectory(Directory);
    }
}
