using System;
using System.IO;
using HUDEditor.Classes;
using SkiaSharp;
using Xunit;

namespace HUDEditor.Tests.Classes;

/// <summary>
/// Tests for converting images to and from VTF textures with vtex2.
/// </summary>
public class VtfTests
{
    [Fact]
    public void Convert_CreatesBackgroundsThatRoundTrip()
    {
        var hudFolder = Directory.CreateTempSubdirectory("hud test ").FullName;
        try
        {
            // Generate a non-square, non-power-of-two source image.
            var source = Path.Combine(hudFolder, "source image.png");
            using (var bitmap = new SKBitmap(300, 170))
            using (var data = bitmap.Encode(SKEncodedImageFormat.Png, 100))
            using (var stream = File.Create(source))
                data.SaveTo(stream);

            VTF.Convert(new Uri(source), hudFolder);

            var console = Path.Combine(hudFolder, "materials", "console");
            Assert.True(File.Exists(Path.Combine(console, "background_upward.vtf")));
            Assert.True(File.Exists(Path.Combine(console, "background_upward_widescreen.vtf")));

            var extracted = Path.Combine(hudFolder, "thumb", "extracted.png");
            VTF.ExtractToPng(Path.Combine(console, "background_upward_widescreen.vtf"), extracted);
            using var result = SKBitmap.Decode(extracted);
            Assert.Equal(512, result.Width);  // Rounded up to the next power of two, square.
            Assert.Equal(512, result.Height);
        }
        finally
        {
            Directory.Delete(hudFolder, true);
        }
    }
}
