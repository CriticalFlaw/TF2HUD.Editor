using HUDEditor.ViewModels;
using Xunit;

namespace HUDEditor.Tests.ViewModels;

public class AppInfoViewModelTests
{
    [Fact]
    public void AppVersion_ReturnsAssemblyVersionOrDefault()
    {
        var vm = new AppInfoViewModel();

        Assert.False(string.IsNullOrWhiteSpace(vm.AppVersion));
        Assert.True(vm.AppVersion.Split('.').Length >= 2); // major.minor
    }
}
