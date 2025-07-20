using Benchy.Infrastructure;

namespace Benchy.Tests.Unit.Infrastructure;

public class DirectoryInfoExtensionsTests
{
    [Fact]
    public void Subdirectory_ValidSubdirectoryName_ReturnsCorrectPath()
    {
        // Arrange
        var baseDir = new DirectoryInfo("/test/base");
        var subdirName = "subdir";

        // Act
        var result = baseDir.Subdirectory(subdirName);

        // Assert
        result.FullName.Should().Be(Path.Combine("/test/base", "subdir"));
    }

    [Fact]
    public void Subdirectory_NestedPath_ReturnsCorrectPath()
    {
        // Arrange
        var baseDir = new DirectoryInfo("/test/base");
        var subdirName = "level1/level2";

        // Act
        var result = baseDir.Subdirectory(subdirName);

        // Assert
        result.FullName.Should().Be(Path.Combine("/test/base", "level1/level2"));
    }

    [Fact]
    public void File_ValidFileName_ReturnsCorrectPath()
    {
        // Arrange
        var baseDir = new DirectoryInfo("/test/base");
        var fileName = "test.txt";

        // Act
        var result = baseDir.File(fileName);

        // Assert
        result.FullName.Should().Be(Path.Combine("/test/base", "test.txt"));
    }

    [Theory]
    [InlineData("")]
    [InlineData(" ")]
    [InlineData("test.txt")]
    [InlineData("file with spaces.json")]
    [InlineData("path/to/file.xml")]
    public void File_VariousFileNames_ReturnsCorrectPath(string fileName)
    {
        // Arrange
        var baseDir = new DirectoryInfo("/test/base");

        // Act
        var result = baseDir.File(fileName);

        // Assert
        result.FullName.Should().Be(Path.Combine("/test/base", fileName));
    }
}

public class TemporaryDirectoryTests
{
    [Fact]
    public void CreateNew_CreatesUniqueDirectories()
    {
        // Arrange & Act
        using var tempDir1 = TemporaryDirectory.CreateNew();
        using var tempDir2 = TemporaryDirectory.CreateNew();

        // Assert
        tempDir1.Directory.FullName.Should().NotBe(tempDir2.Directory.FullName);
        tempDir1.Directory.Exists.Should().BeTrue();
        tempDir2.Directory.Exists.Should().BeTrue();
    }

    [Fact]
    public void CreateNew_DirectoryIsInTempPath()
    {
        // Act
        using var tempDir = TemporaryDirectory.CreateNew();

        // Assert
        var tempPath = Path.GetTempPath();
        tempDir.Directory.FullName.Should().StartWith(tempPath);
        tempDir.Directory.FullName.Should().Contain("Benchy");
    }

    [Fact]
    public void CreateNew_DirectoryIncludesTimestamp()
    {
        // Act
        using var tempDir = TemporaryDirectory.CreateNew();

        // Assert
        var today = DateTime.Now.ToString("yyyy-MM-dd");
        tempDir.Directory.FullName.Should().Contain(today);
    }

    [Fact]
    public void FullName_ReturnsDirectoryFullName()
    {
        // Act
        using var tempDir = TemporaryDirectory.CreateNew();

        // Assert
        tempDir.FullName.Should().Be(tempDir.Directory.FullName);
    }

    [Fact]
    public void CreateSubdirectory_CreatesSubdirectoryInTempDirectory()
    {
        // Arrange
        using var tempDir = TemporaryDirectory.CreateNew();

        // Act
        var subdir = tempDir.CreateSubdirectory("testsubdir");

        // Assert
        subdir.Exists.Should().BeTrue();
        subdir.FullName.Should().Be(Path.Combine(tempDir.FullName, "testsubdir"));
        subdir.Parent?.FullName.Should().Be(tempDir.FullName);
    }

    [Fact]
    public void Dispose_DeletesDirectory()
    {
        // Arrange
        var tempDir = TemporaryDirectory.CreateNew();
        var directoryPath = tempDir.FullName;

        Directory.Exists(directoryPath).Should().BeTrue();

        // Act
        ((IDisposable)tempDir).Dispose();

        // Assert
        Directory.Exists(directoryPath).Should().BeFalse();
    }

    [Fact]
    public void KeepAfterDisposal_PreservesDirectoryAfterDispose()
    {
        // Arrange
        var tempDir = TemporaryDirectory.CreateNew();
        var directoryPath = tempDir.FullName;

        // Act
        tempDir.KeepAfterDisposal();
        ((IDisposable)tempDir).Dispose();

        try
        {
            // Assert
            Directory.Exists(directoryPath).Should().BeTrue();
        }
        finally
        {
            // Cleanup manually since we kept the directory
            if (Directory.Exists(directoryPath))
            {
                Directory.Delete(directoryPath, true);
            }
        }
    }

    [Fact]
    public void Delete_RemovesDirectoryEvenWithFiles()
    {
        // Arrange
        using var tempDir = TemporaryDirectory.CreateNew();
        var testFile = Path.Combine(tempDir.FullName, "test.txt");
        File.WriteAllText(testFile, "test content");

        var subdir = tempDir.CreateSubdirectory("subdir");
        var subFile = Path.Combine(subdir.FullName, "subtest.txt");
        File.WriteAllText(subFile, "sub content");

        // Act
        tempDir.Delete();

        // Assert
        Directory.Exists(tempDir.FullName).Should().BeFalse();
    }

    [Fact]
    public void ToString_ReturnsDirectoryToString()
    {
        // Arrange
        using var tempDir = TemporaryDirectory.CreateNew();

        // Act
        var result = tempDir.ToString();

        // Assert
        result.Should().Be(tempDir.Directory.ToString());
    }

    [Fact]
    public void UsingStatement_AutomaticallyDisposesDirectory()
    {
        // Arrange
        string directoryPath;

        // Act
        using (var tempDir = TemporaryDirectory.CreateNew())
        {
            directoryPath = tempDir.FullName;
            Directory.Exists(directoryPath).Should().BeTrue();
        }

        // Assert
        Directory.Exists(directoryPath).Should().BeFalse();
    }

    [Fact]
    public void Delete_HandlesNonExistentDirectory()
    {
        // Arrange
        using var tempDir = TemporaryDirectory.CreateNew();

        // Delete directory externally first
        Directory.Delete(tempDir.FullName, true);

        // Act & Assert - Should not throw
        var act = () => tempDir.Delete();
        act.Should().NotThrow();
    }
}
