using System;
using HUDEditor.Classes;
using Xunit;

namespace HUDEditor.Tests;

public class UtilitiesMoreTests
{
    [Fact]
    public void CommentTextLine_AddsPrefixAndRemovesExistingCommentMarkers()
    {
        var lines = new[] { "abc", "//already", "x//y" };
        var result0 = Utilities.CommentTextLine(lines, 0);
        var result1 = Utilities.CommentTextLine(lines, 1);
        var result2 = Utilities.CommentTextLine(lines, 2);

        Assert.Equal("//abc", result0);
        Assert.Equal("//already", result1);
        Assert.Equal("//xy", result2);
    }

    [Fact]
    public void UncommentTextLine_RemovesCommentMarkers()
    {
        var lines = new[] { "//abc", "a//b", "normal" };
        Assert.Equal("abc", Utilities.UncommentTextLine(lines, 0));
        Assert.Equal("ab", Utilities.UncommentTextLine(lines, 1));
        Assert.Equal("normal", Utilities.UncommentTextLine(lines, 2));
    }

    [Fact]
    public void GetLineNumbersContainingString_FindsLinesWithSpaceOrTab()
    {
        var lines = new[] { "first line", "has\tvalue", "no match", "value here" };
        var indexes = Utilities.GetLineNumbersContainingString(lines, "has value");
        Assert.Contains(1, indexes);

        var indexes2 = Utilities.GetLineNumbersContainingString(lines, "value here");
        Assert.Contains(3, indexes2);
    }

    [Fact]
    public void ConvertToRgba_TranslatesHexToRgbaString()
    {
        var rgba = Utilities.ConvertToRgba("#112233");
        Assert.Equal("17 34 51 255", rgba);
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
        var result = Utilities.GetShadowColor("100 50 200 128");
        Assert.Equal("60 30 120 255", result);
    }

    [Fact]
    public void GetDimmedColor_SetsAlphaTo100()
    {
        var result = Utilities.GetDimmedColor("5 6 7 255");
        Assert.Equal("5 6 7 100", result);
    }

    [Fact]
    public void GetGrayedColor_ReducesChannelsAndSetsAlpha255()
    {
        var result = Utilities.GetGrayedColor("200 160 120 50");
        Assert.Equal("50 40 30 255", result);
    }

    [Fact]
    public void ConvertToColor_And_ConvertToColorBrush_ReturnValidColors()
    {
        var rgba = "10 20 30 255";
        var color = Utilities.ConvertToColor(rgba);
        // Properties exist and match
        Assert.Equal(10, color.R);
        Assert.Equal(20, color.G);
        Assert.Equal(30, color.B);
        Assert.Equal(255, color.A);

        var brush = Utilities.ConvertToColorBrush(rgba);
        Assert.Equal(color, brush.Color);
    }
}
