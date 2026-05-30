using System.Reflection;
using HUDEditor.ViewModels;
using Xunit;

namespace HUDEditor.Tests;

public class AppInfoViewModelTests
{
    [Fact]
    public void AppVersion_ReturnsAssemblyVersionOrDefault()
    {
        var vm = new AppInfoViewModel();

        // Should not be null or empty
        Assert.False(string.IsNullOrWhiteSpace(vm.AppVersion));

        // Try parse as version parts (major.minor)
        var parts = vm.AppVersion.Split('.');
        Assert.True(parts.Length >= 2);
    }
}
