using SkiaSharp;
using System;
using System.Diagnostics;
using System.IO;
using System.Runtime.InteropServices;

namespace HUDEditor.Classes;

public static class VTF
{
    private static string Vtex2Path => Path.Combine(AppContext.BaseDirectory, RuntimeInformation.IsOSPlatform(OSPlatform.Windows) ? "vtex2.exe" : "vtex2");

    /// <summary>
    /// Converts an image file into a VTF texture file to be used in-game.
    /// </summary>
    /// <param name="image">Path and file name of the image to be converted.</param>
    /// <param name="hudFolderPath">Root folder of the HUD to write the background into.</param>
    /// <exception cref="InvalidOperationException">Thrown if the conversion fails, so callers can roll back.</exception>
    /// <seealso cref="https://github.com/StrataSource/vtex2"/>
    public static void Convert(Uri image, string hudFolderPath)
    {
        var workDir = Directory.CreateDirectory(Path.Combine(Path.GetTempPath(), "TF2HUD.Editor", Guid.NewGuid().ToString("N"))).FullName;
        try
        {
            var tempFile = Path.Combine(workDir, "temp.png");
            var vtfOutput = Path.Combine(workDir, "background_upward.vtf");
            ResizeImage(image.LocalPath, tempFile);

            RunVtex2($"convert --srgb --normal -f dxt1 --no-mips -o \"{vtfOutput}\" \"{tempFile}\"");
            if (!File.Exists(vtfOutput))
                throw new InvalidOperationException("Failed to convert image. Please try a different file.");

            var hudBgPath = Path.Combine(hudFolderPath, "materials", "console");
            Directory.CreateDirectory(hudBgPath);

            // Delete existing background files (the originals were already moved to _disabled by the caller).
            foreach (var file in new DirectoryInfo(hudBgPath).GetFiles())
                File.Delete(file.FullName);

            var output = Path.Combine(hudBgPath, "background_upward.vtf");
            App.Logger.Info($"Copying \"{vtfOutput}\" to \"{output}\"");
            File.Copy(vtfOutput, output, true);
            File.Copy(vtfOutput, Path.Combine(hudBgPath, "background_upward_widescreen.vtf"), true);
        }
        finally
        {
            try { Directory.Delete(workDir, true); } catch { /* best effort */ }
        }
    }

    /// <summary>
    /// Extracts a VTF texture into a PNG image.
    /// </summary>
    public static void ExtractToPng(string vtfPath, string pngPath)
    {
        Directory.CreateDirectory(Path.GetDirectoryName(Path.GetFullPath(pngPath))!);
        RunVtex2($"extract -f png -o \"{pngPath}\" \"{vtfPath}\"");
        if (!File.Exists(pngPath))
            throw new InvalidOperationException($"Failed to extract \"{vtfPath}\".");
    }

    /// <summary>
    /// Resizes an image to a square, power-of-two size and saves it as PNG.
    /// </summary>
    public static void ResizeImage(string inputPath, string outputPath)
    {
        using var image = SKBitmap.Decode(inputPath) ?? throw new InvalidOperationException($"Unsupported image file: {inputPath}");

        // Image size is the greater of both the width and height rounded up to the nearest power of 2
        var size = (int)Math.Max(Math.Pow(2, Math.Ceiling(Math.Log2(image.Width))), Math.Pow(2, Math.Ceiling(Math.Log2(image.Height))));
        ResizeImage(image, size, size, outputPath);
    }

    /// <summary>
    /// Resizes an image to the given dimensions and saves it as PNG.
    /// </summary>
    public static void ResizeImage(string inputPath, int width, int height, string outputPath)
    {
        using var image = SKBitmap.Decode(inputPath) ?? throw new InvalidOperationException($"Unsupported image file: {inputPath}");
        ResizeImage(image, width, height, outputPath);
    }

    private static void ResizeImage(SKBitmap image, int width, int height, string outputPath)
    {
        using var resized = image.Resize(new SKImageInfo(width, height), new SKSamplingOptions(SKFilterMode.Linear, SKMipmapMode.Linear))
            ?? throw new InvalidOperationException("Failed to resize image.");
        using var encoded = resized.Encode(SKEncodedImageFormat.Png, 100);
        using var stream = File.Create(outputPath);
        encoded.SaveTo(stream);
    }

    private static void RunVtex2(string arguments)
    {
        var exe = Vtex2Path;
        if (!File.Exists(exe)) throw new FileNotFoundException("vtex2 was not found next to the application.", exe);

        // The Linux binary loses its executable bit when packaged on Windows; restore it.
        if (!OperatingSystem.IsWindows())
        {
            var mode = File.GetUnixFileMode(exe);
            if (!mode.HasFlag(UnixFileMode.UserExecute))
                File.SetUnixFileMode(exe, mode | UnixFileMode.UserExecute | UnixFileMode.GroupExecute | UnixFileMode.OtherExecute);
        }

        using var process = new Process
        {
            StartInfo = new ProcessStartInfo
            {
                FileName = exe,
                Arguments = arguments,
                WorkingDirectory = AppContext.BaseDirectory,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                UseShellExecute = false,
                CreateNoWindow = true
            }
        };

        process.Start();
        // Read both streams asynchronously so a full buffer can't deadlock the process.
        var stdout = process.StandardOutput.ReadToEndAsync();
        var stderr = process.StandardError.ReadToEndAsync();
        process.WaitForExit();

        if (!string.IsNullOrWhiteSpace(stdout.Result)) App.Logger.Info($"vtex2: {stdout.Result.Trim()}");
        if (process.ExitCode != 0)
            throw new InvalidOperationException($"vtex2 exited with code {process.ExitCode}: {stderr.Result.Trim()}");
    }
}
