using System;
using System.IO;
using HUDEditor.Classes;
using Xunit;

namespace HUDEditor.Tests.Classes;

/// <summary>
/// Tests for the path helpers in <see cref="Utilities"/>.
/// </summary>
public class PathUtilitiesTests
{
    [Fact]
    public void GetTfDirectory_HandlesBothSeparatorStyles()
    {
        var root = Path.Combine(Path.GetTempPath(), "Team Fortress 2", "tf");
        var custom = Path.Combine(root, "custom");

        Assert.Equal(Path.GetFullPath(root), Utilities.GetTfDirectory(custom));
        Assert.Equal(Path.GetFullPath(root), Utilities.GetTfDirectory(custom.Replace('\\', '/') + "/"));
    }

    [Fact]
    public void ToLocalPath_DecodesFileUris()
    {
        var path = Path.Combine(Path.GetTempPath(), "my hud", "my hud.zip");
        var uri = new Uri(path).AbsoluteUri; // Contains %20

        Assert.Contains("%20", uri);
        Assert.Equal(path, Utilities.ToLocalPath(uri));
    }

    [Fact]
    public void ToLocalPath_ReturnsPlainPathsUnchanged()
    {
        Assert.Equal("relative/path.png", Utilities.ToLocalPath("relative/path.png"));
    }
}
