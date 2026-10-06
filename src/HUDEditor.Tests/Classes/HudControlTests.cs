using System.Collections.Generic;
using HUDEditor.Classes;
using HUDEditor.Models;
using Xunit;

namespace HUDEditor.Tests.Classes;

/// <summary>
/// Tests for how HUD schema controls are identified and interpreted.
/// </summary>
public class HudControlTests
{
    [Theory]
    [InlineData("123abc", "_123abc")]
    [InlineData("my-hud name", "my_hud_name")]
    public void EncodeId_TransformsIdsCorrectly(string input, string expected)
    {
        Assert.Equal(expected, Utilities.EncodeId(input));
    }

    [Theory]
    [InlineData("flawhud_fh_val_xhair_size")]
    [InlineData("_7hud_val_xhair_size")]
    public void EncodeId_IsIdempotent(string id)
    {
        Assert.Equal(Utilities.EncodeId(id), Utilities.EncodeId(Utilities.EncodeId(id)));
    }

    [Fact]
    public void GetFileNames_ReturnsComboFiles_WhenComboDirectoriesIsEmpty()
    {
        // Schema arrays default to empty, which previously hid ComboFiles entirely.
        var control = new Controls { ComboFiles = ["a.res", "b.res"] };

        Assert.Equal(new[] { "a.res", "b.res" }, (string[])Utilities.GetFileNames(control)!);
    }

    [Fact]
    public void GetFileNames_PrefersComboDirectories_ThenFileName()
    {
        Assert.Equal(new[] { "dir" }, (string[])Utilities.GetFileNames(new Controls { ComboDirectories = ["dir"], ComboFiles = ["a.res"] })!);
        Assert.Equal("file", (string)Utilities.GetFileNames(new Controls { FileName = "file.res" })!);
        Assert.Null(Utilities.GetFileNames(new Controls()));
    }

    [Theory]
    [InlineData("fh_val_xhair_size", "IntegerUpDown", Utilities.CrosshairSetting.Size)]
    [InlineData("bh_size_healthammomain", "IntegerUpDown", Utilities.CrosshairSetting.None)]
    [InlineData("zh_dmgnum_size", "IntegerUpDown", Utilities.CrosshairSetting.None)]
    [InlineData("fh_toggle_xhair_enable", "CheckBox", Utilities.CrosshairSetting.Enabled)]
    [InlineData("fh_toggle_xhair_pulse", "CheckBox", Utilities.CrosshairSetting.None)]
    [InlineData("mm_color_xhair_visibility", "CheckBox", Utilities.CrosshairSetting.Enabled)]
    [InlineData("fh_val_xhair_style", "Crosshair", Utilities.CrosshairSetting.Style)]
    [InlineData("kbn_hitmarker_style", "Crosshair", Utilities.CrosshairSetting.None)]
    [InlineData("fh_color_xhair_normal", "ColorPicker", Utilities.CrosshairSetting.Color)]
    [InlineData("eve_color_xhair", "ColorPicker", Utilities.CrosshairSetting.Color)]
    [InlineData("eve_color_xhair_hitmarker", "ColorPicker", Utilities.CrosshairSetting.None)]
    [InlineData("myhud_val_xhair_size", "IntegerUpDown", Utilities.CrosshairSetting.Size)]
    public void GetCrosshairSetting_MatchesOnlyCrosshairControls(string name, string type, Utilities.CrosshairSetting expected)
    {
        Assert.Equal(expected, Utilities.GetCrosshairSetting(new Controls { Name = name, Type = type }));
    }

    [Fact]
    public void CrosshairStyles_ContainsEveryGlyphOnce()
    {
        Assert.Contains("l", Utilities.CrosshairStyles);
        Assert.Equal(Utilities.CrosshairStyles.Count, new HashSet<string>(Utilities.CrosshairStyles).Count);
    }
}
