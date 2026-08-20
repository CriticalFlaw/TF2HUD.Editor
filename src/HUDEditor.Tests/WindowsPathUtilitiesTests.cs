using System;
using System.IO;
using HUDEditor.Classes;
using Xunit;

namespace HUDEditor.Tests;

/// <summary>
/// Tests for Windows file system and path utilities.
/// </summary>
public class WindowsPathUtilitiesTests
{
    [Fact]
    [Trait("Category", "Windows")]
    public void PathCombine_ConstructsValidTF2Paths()
    {
        // Arrange
        var baseLibrary = @"C:\Program Files\Steam";
        var steamCustomPath = @"steamapps\common\Team Fortress 2\tf\custom";

        // Act
        var fullPath = Path.Combine(baseLibrary, steamCustomPath);

        // Assert
        Assert.Equal(@"C:\Program Files\Steam\steamapps\common\Team Fortress 2\tf\custom", fullPath);
    }

    [Fact]
    [Trait("Category", "Windows")]
    public void PathEndsWith_ChecksPathSuffix()
    {
        // Arrange
        var validPath = @"C:\Steam\steamapps\common\Team Fortress 2\tf\custom";
        var invalidPath = @"C:\Steam\steamapps\common\Team Fortress 2\tf";

        // Act & Assert
        Assert.True(validPath.EndsWith("tf\\custom"));
        Assert.False(invalidPath.EndsWith("tf\\custom"));
    }

    [Fact]
    [Trait("Category", "Windows")]
    public void RelativePathWithEscapeSequences()
    {
        // Arrange: VDF format uses double backslashes
        var vdfPath = @"C:\\Program Files\\Steam";
        var normalized = vdfPath.Replace(@"\\", @"\");

        // Act & Assert
        Assert.Equal(@"C:\Program Files\Steam", normalized);
    }

    [Fact]
    [Trait("Category", "Windows")]
    public void TempFileOperations_CreateAndDelete()
    {
        // Arrange
        var tempFile = Path.Combine(Path.GetTempPath(), "test_" + Guid.NewGuid().ToString() + ".txt");

        try
        {
            // Act: Create file
            File.WriteAllText(tempFile, "test content");
            Assert.True(File.Exists(tempFile));

            // Act: Read file
            var content = File.ReadAllText(tempFile);
            Assert.Equal("test content", content);

            // Act: Delete file
            File.Delete(tempFile);
            Assert.False(File.Exists(tempFile));
        }
        finally
        {
            if (File.Exists(tempFile))
                File.Delete(tempFile);
        }
    }

    [Fact]
    [Trait("Category", "Windows")]
    public void ReadLines_ParsesVdfStyleContent()
    {
        // Arrange
        var vdfContent = @"""path"" ""C:\Steam""
""path"" ""D:\Games""
""other"" ""value""";
        var tempFile = Path.Combine(Path.GetTempPath(), "test_vdf_" + Guid.NewGuid().ToString());
        File.WriteAllText(tempFile, vdfContent);

        try
        {
            // Act
            var lines = File.ReadLines(tempFile);
            var lineList = new System.Collections.Generic.List<string>(lines);

            // Assert
            Assert.Equal(3, lineList.Count);
            Assert.All(lineList, line => Assert.NotEmpty(line));
        }
        finally
        {
            if (File.Exists(tempFile))
                File.Delete(tempFile);
        }
    }

    [Fact]
    [Trait("Category", "Windows")]
    public void DirectoryPath_Construction()
    {
        // Arrange
        var parent = @"C:\Program Files";
        var child = "Steam";

        // Act
        var combined = Path.Combine(parent, child);

        // Assert
        Assert.Equal(@"C:\Program Files\Steam", combined);
        Assert.True(combined.Contains("Program Files"));
        Assert.True(combined.Contains("Steam"));
    }

    [Fact]
    [Trait("Category", "Windows")]
    public void GetTempPath_ReturnsValidDirectory()
    {
        // Act
        var tempPath = Path.GetTempPath();

        // Assert
        Assert.NotEmpty(tempPath);
        Assert.True(Directory.Exists(tempPath));
    }

    [Fact]
    [Trait("Category", "Windows")]
    public void StringNullOrWhitespace_Validation()
    {
        // Arrange & Act & Assert
        Assert.True(string.IsNullOrWhiteSpace(null));
        Assert.True(string.IsNullOrWhiteSpace(string.Empty));
        Assert.True(string.IsNullOrWhiteSpace("   "));
        Assert.False(string.IsNullOrWhiteSpace(@"C:\Path"));
    }

    [Fact]
    [Trait("Category", "Windows")]
    public void FileSystemPathVariations_AreHandled()
    {
        // Arrange: Different path variations
        var paths = new[]
        {
            @"C:\Games\TF2",
            @"D:\SteamLibrary\TF2",
            @"\\network\share\TF2",  // UNC path
        };

        // Act & Assert: All should be valid path strings
        foreach (var path in paths)
        {
            Assert.NotEmpty(path);
            Assert.False(string.IsNullOrWhiteSpace(path));
        }
    }
}
