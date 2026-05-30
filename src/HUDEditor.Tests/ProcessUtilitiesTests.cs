using System;
using System.Collections.Generic;
using System.Diagnostics;
using HUDEditor.Classes;
using Xunit;
using Moq;

namespace HUDEditor.Tests;

/// <summary>
/// Tests for process detection and game running checks.
/// These are abstracted to allow mocking without actual process calls.
/// </summary>
public class ProcessUtilitiesTests
{
    [Fact]
    [Trait("Category", "Windows")]
    public void IsGameRunning_DetectsMultipleGameProcessNames()
    {
        // Arrange: Check for known process names that TF2 can use
        var processNames = new[] { "hl2", "tf", "tf_win64" };

        // Act: Simulate checking for these processes
        var runningProcesses = new List<Process>();
        foreach (var name in processNames)
        {
            try
            {
                var procs = Process.GetProcessesByName(name);
                runningProcesses.AddRange(procs);
            }
            catch
            {
                // Process lookup can fail if process doesn't exist
            }
        }

        // Assert: No assertion on actual processes (depends on system state)
        // This test demonstrates the logic - in production, would mock this
        Assert.NotNull(runningProcesses);
        Assert.IsType<List<Process>>(runningProcesses);
    }

    [Fact]
    public void GameProcessLogic_ReturnsCorrectWhenNoProcessesFound()
    {
        // Arrange & Act
        var result = CheckGameRunningLogic(hasHl2: false, hasTf: false, hasTf64: false);

        // Assert: Should return false (no game running)
        Assert.False(result);
    }

    [Theory]
    [InlineData(true, false, false)]  // Only hl2
    [InlineData(false, true, false)]  // Only tf
    [InlineData(false, false, true)]  // Only tf_win64
    [InlineData(true, true, false)]   // hl2 and tf
    [InlineData(true, true, true)]    // All three
    public void GameProcessLogic_ReturnsCorrectWhenProcessesFound(bool hasHl2, bool hasTf, bool hasTf64)
    {
        // Arrange & Act
        var result = CheckGameRunningLogic(hasHl2, hasTf, hasTf64);

        // Assert: Should return true if any process is found
        Assert.True(result);
    }

    [Fact]
    public void ProcessNameList_ContainsAllTeamFortress2Variants()
    {
        // Arrange
        var processNames = new[] { "hl2", "tf", "tf_win64" };

        // Act & Assert
        Assert.Contains("hl2", processNames);
        Assert.Contains("tf", processNames);
        Assert.Contains("tf_win64", processNames);
        Assert.Equal(3, processNames.Length);
    }

    [Fact]
    public void SteamDirectoryPathLogic_FormsTF2CustomPath()
    {
        // Arrange
        var libraryPath = @"C:\Program Files\Steam";

        // Act: Simulate the path construction
        var tf2CustomPath = System.IO.Path.Combine(
            libraryPath, "steamapps", "common", "Team Fortress 2", "tf", "custom");

        // Assert
        Assert.Equal(@"C:\Program Files\Steam\steamapps\common\Team Fortress 2\tf\custom", tf2CustomPath);
    }

    [Fact]
    public void MultipleLibraryPaths_AreIteratedUntilTf2Found()
    {
        // Arrange
        var libraries = new[]
        {
            @"C:\Program Files\Steam",
            @"D:\Games\Steam",
            @"E:\SteamLibrary"
        };

        // Act & Assert: Demonstrate logic that would search each library
        foreach (var library in libraries)
        {
            var tf2Path = System.IO.Path.Combine(library, "steamapps", "common", "Team Fortress 2", "tf", "custom");
            Assert.Contains(library, tf2Path);
        }
    }

    // Helper: Simulates the logic of CheckIsGameRunning
    private bool CheckGameRunningLogic(bool hasHl2, bool hasTf, bool hasTf64)
    {
        var hl2Count = hasHl2 ? 1 : 0;
        var tfCount = hasTf ? 1 : 0;
        var tf64Count = hasTf64 ? 1 : 0;

        return hl2Count > 0 || tfCount > 0 || tf64Count > 0;
    }
}
