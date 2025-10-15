using System;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Runtime.InteropServices;

namespace HUDEditor.Classes;

public static class VTF
{
    /// <summary>
    /// Converts an image file into a VTF texture file to be used in-game.
    /// </summary>
    /// <param name="image">Path and file name of the image to be converted.</param>
    /// <seealso cref="https://github.com/StrataSource/vtex2"/>
    public static void Convert(Uri image)
    {
        var tempFile = Path.Combine(AppContext.BaseDirectory, "temp.png");
        ResizeImage(new Bitmap(image.LocalPath)).Save(tempFile);

        var process = new Process
        {
            StartInfo = new ProcessStartInfo
            {
                FileName = Path.Combine(AppContext.BaseDirectory, RuntimeInformation.IsOSPlatform(OSPlatform.Linux) ? "vtex2" : "vtex2.exe"),
                Arguments = $"convert --srgb --normal -f dxt1 --no-mips -o background_upward.vtf \"{tempFile}\"",
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                UseShellExecute = false,
                CreateNoWindow = true
            }
        };

        process.Start();
        process.WaitForExit();

        ///-----

        // Path to VTEX output file
        var vtfOutput = Path.Combine(AppContext.BaseDirectory, "background_upward.vtf");

        if (!File.Exists(vtfOutput))
        {
            _ = Utilities.ShowMessageBox("Failed to convert image. Please try a different file.", MsBox.Avalonia.Enums.Icon.Error);
            return;
        }

        var hudBgPath = $"{App.HudPath}/{App.Config.ConfigSettings.UserPrefs.SelectedHUD}/materials/console/";

        // Delete existing background files.
        foreach (var file in new DirectoryInfo(hudBgPath).GetFiles())
            File.Delete(file.FullName);

        // Move the final vtf to user defined path
        var output = Path.Combine(hudBgPath, "background_upward.vtf");
        App.Logger.Info($"Copying \"{output}\" to \"{hudBgPath}\"");
        File.Move(vtfOutput, output, true);
        File.Copy(output, output.Replace("background_upward", "background_upward_widescreen"), true);

        // Clean up temp files
        File.Delete(tempFile);
        File.Delete(vtfOutput);
    }

    /// <summary>
    /// Resizes the image file so it can be saved into Targa file.
    /// </summary>
    /// <param name="image">Bitmap of the input image file.</param>
    /// <returns>Bitmap of the resized image file.</returns>
    public static Bitmap ResizeImage(Bitmap image)
    {
        // Image size is the greater of both the width and height rounded up to the nearest power of 2
        var size = (int)Math.Max(Math.Pow(2, Math.Ceiling(Math.Log(image.Width) / Math.Log(2))), Math.Pow(2, Math.Ceiling(Math.Log(image.Height) / Math.Log(2))));

        // Paint graphics onto the SquareImage Bitmap
        var squareImage = new Bitmap(size, size);
        var graphics = Graphics.FromImage(squareImage);
        graphics.DrawImage(image, 0, 0, size, size);
        return squareImage;
    }
}