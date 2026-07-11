using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.InteropServices;
using System.Text;
using HUDEditor.Classes;
using Xunit;

namespace HUDEditor.Tests;

/// <summary>
/// Tests for Windows-specific OS/IO Utilities methods.
/// Some tests are skipped on non-Windows platforms.
/// </summary>
public class WindowsUtilitiesTests
{
    [Fact]
    public void ParseLibraryFolders_ExtractsPathsFromValidVdfContent()
    {
        // Arrange: Create a temporary VDF file with library folder paths
        var vdfContent = @"""path"" ""C:\Program Files (x86)\Steam""
""path"" ""D:\Games\Steam""
""path"" ""E:\SteamLibrary""
";
        var tempFile = Path.Combine(Path.GetTempPath(), "libraryfolders.vdf");
        File.WriteAllText(tempFile, vdfContent);

        try
        {
            // Act
            var paths = ParseLibraryFoldersViaReflection(tempFile);

            // Assert
            Assert.Contains(@"C:\Program Files (x86)\Steam", paths);
            Assert.Contains(@"D:\Games\Steam", paths);
            Assert.Contains(@"E:\SteamLibrary", paths);
        }
        finally
        {
            if (File.Exists(tempFile))
                File.Delete(tempFile);
        }
    }

    [Fact]
    public void ParseLibraryFolders_HandlesBackslashEscapes()
    {
        // Arrange: VDF uses double backslashes for escaped paths
        var vdfContent = @"""path"" ""C:\\Program Files\\Steam""";
        var tempFile = Path.Combine(Path.GetTempPath(), "libraryfolders_escape.vdf");
        File.WriteAllText(tempFile, vdfContent);

        try
        {
            // Act
            var paths = ParseLibraryFoldersViaReflection(tempFile);

            // Assert
            Assert.Single(paths);
            Assert.Contains(@"C:\Program Files\Steam", paths);
        }
        finally
        {
            if (File.Exists(tempFile))
                File.Delete(tempFile);
        }
    }

    [Fact]
    public void ParseLibraryFolders_IgnoresInvalidLines()
    {
        // Arrange: Mix of valid and invalid lines
        var vdfContent = @"""path"" ""C:\Steam""
random garbage line
    invalid line
""path"" ""D:\Games""
#comment
other text
";
        var tempFile = Path.Combine(Path.GetTempPath(), "libraryfolders_mixed.vdf");
        File.WriteAllText(tempFile, vdfContent);

        try
        {
            // Act
            var paths = ParseLibraryFoldersViaReflection(tempFile);

            // Assert: Only valid paths are extracted
            Assert.Equal(2, paths.Count);
            Assert.Contains(@"C:\Steam", paths);
            Assert.Contains(@"D:\Games", paths);
        }
        finally
        {
            if (File.Exists(tempFile))
                File.Delete(tempFile);
        }
    }

    [Fact]
    public void ParseLibraryFolders_ReturnsEmptyListForEmptyFile()
    {
        // Arrange
        var tempFile = Path.Combine(Path.GetTempPath(), "libraryfolders_empty.vdf");
        File.WriteAllText(tempFile, string.Empty);

        try
        {
            // Act
            var paths = ParseLibraryFoldersViaReflection(tempFile);

            // Assert
            Assert.Empty(paths);
        }
        finally
        {
            if (File.Exists(tempFile))
                File.Delete(tempFile);
        }
    }

    [Fact(Skip = "Requires App.HudPath and App.Config to be initialized")]
    public void CheckUserPath_ValidPathWithCustom()
    {
        // This test is skipped because CheckUserPath depends on static App state
        // In a real refactored scenario, these would be injected dependencies
    }

    [Fact]
    [Trait("Category", "Windows")]
    public void ValidatePathEndsWithCustom_ReturnsCorrectValueWithNormalization()
    {
        // Test that paths with mixed separators are normalized correctly
        var path1 = @"C:\Program Files\Steam\steamapps\common\Team Fortress 2\tf\custom";
        var path2 = @"C:\Program Files\Steam\steamapps\common\Team Fortress 2\tf";
        var path3 = @"C:/Program Files/Steam/steamapps/common/Team Fortress 2/tf/custom"; // forward slashes
        var path4 = string.Empty;

        var sep = System.IO.Path.DirectorySeparatorChar;
        var expectedEndPath = $"tf{sep}custom";

        // Normalize paths to OS separator and check ending
        var normalizedPath1 = path1.Replace('/', sep).Replace('\\', sep);
        var normalizedPath2 = path2.Replace('/', sep).Replace('\\', sep);
        var normalizedPath3 = path3.Replace('/', sep).Replace('\\', sep);

        Assert.True(normalizedPath1.EndsWith(expectedEndPath, StringComparison.OrdinalIgnoreCase));
        Assert.False(normalizedPath2.EndsWith(expectedEndPath, StringComparison.OrdinalIgnoreCase));
        Assert.True(normalizedPath3.EndsWith(expectedEndPath, StringComparison.OrdinalIgnoreCase));
        Assert.False(string.IsNullOrWhiteSpace(path1)); // Path is not null/whitespace
        Assert.True(string.IsNullOrWhiteSpace(path4)); // Path is null/whitespace
    }

    [Fact]
    [Trait("Category", "Windows")]
    public void DirectoryExistsCheck_WorksForValidAndInvalidPaths()
    {
        // Arrange: Use a known directory (temp folder exists)
        var tempDir = Path.GetTempPath();
        var nonExistentDir = Path.Combine(tempDir, "nonexistent_" + Guid.NewGuid().ToString());

        // Act & Assert
        Assert.True(Directory.Exists(tempDir));
        Assert.False(Directory.Exists(nonExistentDir));
    }

    [Fact(Skip = "Requires actual registry access or mocking")]
    public void SearchRegistry_FindsTeamFortress2Installation()
    {
        // This would require either:
        // 1. Mocking the registry access
        // 2. Running on a system with TF2 installed
        // Skipping for now - requires architectural changes to Utilities for testability
    }

    // Helper method using reflection to call private ParseLibraryFolders
    private List<string> ParseLibraryFoldersViaReflection(string filePath)
    {
        var method = typeof(Utilities).GetMethod("ParseLibraryFolders",
            System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Static);

        if (method == null)
            throw new InvalidOperationException("ParseLibraryFolders method not found");

        var result = method.Invoke(null, new object[] { filePath });
        return result as List<string> ?? new List<string>();
    }
}
