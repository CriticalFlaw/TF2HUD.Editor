using System.Collections.Generic;
using HUDEditor.Models;
using HUDEditor.Classes;
using Xunit;

namespace HUDEditor.Tests;

public class AppJsonAndUtilitiesTests
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

    [Theory]
    [InlineData("123abc", "_123abc")]
    [InlineData("my-hud name", "my_hud_name")]
    public void EncodeId_TransformsIdsCorrectly(string input, string expected)
    {
        var actual = Utilities.EncodeId(input);
        Assert.Equal(expected, actual);
    }

    [Fact]
    public void Merge_OverridesValuesFromSecondDictionary()
    {
        var d1 = new Dictionary<string, dynamic>
        {
            { "a", 1 },
            { "b", new Dictionary<string, dynamic> { { "x", 10 }, { "y", 20 } } }
        };

        var d2 = new Dictionary<string, dynamic>
        {
            { "a", 2 },
            { "b", new Dictionary<string, dynamic> { { "x", 30 } } }
        };

        Utilities.Merge(d1, d2);

        Assert.Equal(2, (int)d1["a"]);
        var nested = (Dictionary<string, dynamic>)d1["b"];
        Assert.Equal(30, (int)nested["x"]);
        Assert.Equal(20, (int)nested["y"]); // preserved
    }
}
