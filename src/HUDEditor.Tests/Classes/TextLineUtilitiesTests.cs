using HUDEditor.Classes;
using Xunit;

namespace HUDEditor.Tests.Classes;

/// <summary>
/// Tests for the line-based helpers used by the "comment"/"uncomment" animation customizations.
/// </summary>
public class TextLineUtilitiesTests
{
    [Fact]
    public void CommentTextLine_AddsPrefixAndRemovesExistingCommentMarkers()
    {
        var lines = new[] { "abc", "//already", "x//y" };

        Assert.Equal("//abc", Utilities.CommentTextLine(lines, 0));
        Assert.Equal("//already", Utilities.CommentTextLine(lines, 1));
        Assert.Equal("//xy", Utilities.CommentTextLine(lines, 2));
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

        Assert.Contains(1, Utilities.GetLineNumbersContainingString(lines, "has value"));
        Assert.Contains(3, Utilities.GetLineNumbersContainingString(lines, "value here"));
    }
}
