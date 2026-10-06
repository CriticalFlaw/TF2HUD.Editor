using HUDEditor.Models;
using Xunit;

namespace HUDEditor.Tests.Models;

public class ConfigurationModelTests
{
    [Fact]
    public void ConfigurationModel_Defaults_AreSet()
    {
        var cfg = new ConfigurationModel();

        Assert.NotNull(cfg.ConfigSettings);
        Assert.NotNull(cfg.ConfigSettings.UserPrefs);
        Assert.NotNull(cfg.ConfigSettings.AppConfig);
        Assert.True(cfg.ConfigSettings.UserPrefs.AutoUpdate);
    }
}
