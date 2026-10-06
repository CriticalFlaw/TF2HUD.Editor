using System.Collections.Generic;
using HUDEditor.Classes;
using Xunit;

namespace HUDEditor.Tests.Classes;

/// <summary>
/// Tests for reading, writing and merging Valve KeyValues (.res/.vdf) data.
/// </summary>
public class VdfTests
{
    [Fact]
    public void Stringify_WritesConditionalListKeysWithoutDelimiter()
    {
        const string input = "\"Root\"\r\n{\r\n\t\"xpos\"\t\"10\" [$WIN32]\r\n\t\"xpos\"\t\"20\" [$WIN32]\r\n}\r\n";

        var output = VDF.Stringify(VDF.Parse(input));

        Assert.DoesNotContain("^", output);
        Assert.Contains("\"xpos\"\t\"10\" [$WIN32]", output);

        // Round-trips to the same structure.
        var reparsed = VDF.Parse(output);
        Assert.Equal(2, ((List<dynamic>)reparsed["Root"]["xpos^[$WIN32]"]).Count);
    }

    [Fact]
    public void Stringify_QuotesKeysOfRepeatedObjects()
    {
        const string input = "\"Root\" { \"Some Name\" { \"a\" \"1\" } \"Some Name\" { \"a\" \"2\" } }";

        var reparsed = VDF.Parse(VDF.Stringify(VDF.Parse(input)));

        Assert.Equal(2, ((List<dynamic>)reparsed["Root"]["Some Name"]).Count);
    }

    [Fact]
    public void Parse_UnterminatedQuote_ThrowsSyntaxError()
    {
        Assert.Throws<VDFSyntaxException>(() => VDF.Parse("\"Root\" { \"key\" \"value"));
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
