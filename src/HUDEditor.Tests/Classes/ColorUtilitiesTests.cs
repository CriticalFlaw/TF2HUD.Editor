using HUDEditor.Classes;
using Xunit;

namespace HUDEditor.Tests.Classes;

/// <summary>
/// Tests for the color conversion helpers in <see cref="Utilities"/>.
/// </summary>
public class ColorUtilitiesTests
{
    [Fact]
    public void ConvertToRgba_TranslatesHexToRgbaString()
    {
        Assert.Equal("17 34 51 255", Utilities.ConvertToRgba("#112233"));
    }

    [Fact]
    public void ConvertToColor_And_ConvertToColorBrush_ReturnMatchingColors()
    {
        const string rgba = "10 20 30 255";

        var color = Utilities.ConvertToColor(rgba);
        Assert.Equal(10, color.R);
        Assert.Equal(20, color.G);
        Assert.Equal(30, color.B);
        Assert.Equal(255, color.A);

        var brush = Utilities.ConvertToColorBrush(rgba);
        Assert.Equal(color, brush.Color);
    }

    [Fact]
    public void GetPulsedColor_DecreasesAlphaWhenAboveThreshold()
    {
        Assert.Equal("10 20 30 150", Utilities.GetPulsedColor("10 20 30 200"));
        Assert.Equal("10 20 30 30", Utilities.GetPulsedColor("10 20 30 30"));
    }

    [Fact]
    public void GetShadowColor_DarkensChannelsAndSetsAlpha255()
    {
        Assert.Equal("60 30 120 255", Utilities.GetShadowColor("100 50 200 128"));
    }

    [Fact]
    public void GetDimmedColor_SetsAlphaTo100()
    {
        Assert.Equal("5 6 7 100", Utilities.GetDimmedColor("5 6 7 255"));
    }

    [Fact]
    public void GetGrayedColor_ReducesChannelsAndSetsAlpha255()
    {
        Assert.Equal("50 40 30 255", Utilities.GetGrayedColor("200 160 120 50"));
    }
}
