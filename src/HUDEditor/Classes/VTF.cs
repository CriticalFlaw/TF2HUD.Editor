using Crews.Utility.TgaSharp;
using System;
using System.Diagnostics;
using System.Drawing;
using System.IO;

namespace HUDEditor.Classes;

internal class VTF
{
    private readonly string Tf2Path;

    public VTF(string path)
    {
        Tf2Path = path.Replace("/tf/custom", string.Empty);
    }

    /// <summary>
    /// Converts an image file into a VTF texture file to be used in-game.
    /// </summary>
    /// <param name="inFile">Path and file name of the image to be converted.</param>
    /// <param name="outFile">Output path and file name for the VTF.</param>
    public void Convert(Uri inFile, string outFile)
    {
        // Resize image to square of larger proportional
        var image = new Bitmap(inFile.LocalPath);
        var materialSrc = $"{Tf2Path}/tf/materialsrc";

        // Create materialsrc (ensure it exists)
        if (!Directory.Exists(materialSrc))
            Directory.CreateDirectory(materialSrc);

        // Save image as .TGA (cast using TGASharpLib)
        var tgaSquareImage = (TGA)ResizeImage(image);
        tgaSquareImage.Save($"{materialSrc}/temp.tga");

        // Convert using VTEX
        VtexConvert(materialSrc, "temp");

        // Path to VTEX output file
        var vtfOutput = $"{Tf2Path}/tf/materials/temp.vtf";

        // Create absolute path to output folder and make directory
        var pathInfo = outFile.Split('/', '/');
        pathInfo[^1] = "";
        var directory = string.Join("/", pathInfo);
        if (!Directory.Exists(directory))
            Directory.CreateDirectory(directory);

        // Make a backup of the existing background files.
        var hudBgPath = new DirectoryInfo($"{App.HudPath}/{App.Config.ConfigSettings.UserPrefs.SelectedHUD}/materials/console/");
        foreach (var file in hudBgPath.GetFiles())
            File.Delete(file.FullName);

        // Copy vtf from vtex output to user defined path
        File.Copy(vtfOutput, outFile, true);

        // Delete temporary tga and vtex output
        File.Delete($"{materialSrc}/temp.tga");
        File.Delete($"{materialSrc}/temp.txt");
        File.Delete(vtfOutput);
    }

    /// <summary>
    /// Resizes the image file so it can be saved into Targa file.
    /// </summary>
    /// <param name="image">Bitmap of the input image file.</param>
    /// <returns>Bitmap of the resized image file.</returns>
    private static Bitmap ResizeImage(Bitmap image)
    {
        // Image size is the greater of both the width and height rounded up to the nearest power of 2
        var size = (int)Math.Max(Math.Pow(2, Math.Ceiling(Math.Log(image.Width) / Math.Log(2))), Math.Pow(2, Math.Ceiling(Math.Log(image.Height) / Math.Log(2))));

        // Paint graphics onto the SquareImage Bitmap
        var squareImage = new Bitmap(size, size);
        var graphics = Graphics.FromImage(squareImage);
        graphics.DrawImage(image, 0, 0, size, size);
        return squareImage;
    }

    /// <summary>
    /// Uses the Valve Texture Tool (Vtex) to convert a Targa file (TGA) into a Valve Texture File (VTF).
    /// </summary>
    /// <param name="folderPath">Input image file path.</param>
    /// <param name="fileName">Name of the image to be converted.</param>
    /// <remarks>See: https://developer.valvesoftware.com/wiki/Vtex_compile_parameters </remarks>
    private void VtexConvert(string folderPath, string fileName)
    {
        // Set the VTEX Args
        File.WriteAllLines($"{folderPath}/{fileName}.txt",
        [
            "pointsample 1",
            "nolod 1",
            "nomip 1"
        ]);

        // Set the VTEX CLI Args
        string[] args =
        [
            "-quiet",
            $"\"{folderPath}/{fileName}.tga\""
        ];

        // Call Vtex and pass the parameters.
        var processInfo = new ProcessStartInfo($"{Tf2Path}/bin/vtex.exe")
        {
            Arguments = string.Join(" ", args),
            RedirectStandardOutput = true
        };
        var process = Process.Start(processInfo);
        while (!process.StandardOutput.EndOfStream) App.Logger.Info(process.StandardOutput.ReadLine());
        process.WaitForExit();
        process.Close();
        process.Dispose();
    }
}