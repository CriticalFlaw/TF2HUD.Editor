using System;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using TF2HUD.Editor.Properties;

namespace TF2HUD.Editor.Classes
{
    internal class VTF
    {
        private readonly string tf2Path;

        public VTF(string tf2Path)
        {
            this.tf2Path = tf2Path.Replace("\\tf\\custom", string.Empty);
        }

        public void Convert(string inFile, string outFile)
        {
            // Resize image to square of larger proportional
            var image = new Bitmap(inFile);
            var materialSrc = $"{tf2Path}\\tf\\materialsrc";

            // Create materialsrc (ensure it exists)
            Directory.CreateDirectory(materialSrc);

            // Save image as .tga (cast using TGASharpLib)
            var tgaSquareImage = (TGA) ResizeImage(image);
            tgaSquareImage.Save($"{materialSrc}\\temp.tga");

            // Convert using VTEX
            VtexConvert(materialSrc, "temp");

            // Path to VTEX output file
            var vtfOutput = $"{tf2Path}\\tf\\materials\\temp.vtf";

            // Create absolute path to output folder and make directory
            var pathInfo = outFile.Split('\\', '/');
            pathInfo[^1] = "";
            Directory.CreateDirectory(string.Join("\\", pathInfo));

            // Make a backup of the existing background files.
            var hudBgPath =
                new DirectoryInfo(string.Format(Resources.path_console, MainWindow.HudPath, MainWindow.HudSelection));
            foreach (var file in hudBgPath.GetFiles())
                File.Delete(file.FullName);

            // Copy vtf from vtex output to user defined path
            File.Copy(vtfOutput, outFile, true);

            // Delete temporary tga and vtex output
            File.Delete($"{materialSrc}\\temp.tga");
            File.Delete($"{materialSrc}\\temp.txt");
            File.Delete(vtfOutput);
        }

        private static Bitmap ResizeImage(Bitmap image)
        {
            var power = 2;
            do
            {
                power *= 2;
            } while (power < Math.Max(image.Width, image.Height));

            // Paint graphics onto the SquareImage Bitmap
            var squareImage = new Bitmap(power, power);
            var graphics = Graphics.FromImage(squareImage);
            graphics.DrawImage(image, 0, 0, power, power);
            return squareImage;
        }

        /// <summary>
        ///     https://developer.valvesoftware.com/wiki/Vtex_compile_parameters
        /// </summary>
        private void VtexConvert(string folderPath, string fileName)
        {
            // VTEX Args
            File.WriteAllLines($"{folderPath}\\{fileName}.txt", new[]
            {
                "pointsample 1",
                "nolod 1",
                "nomip 1"
            });

            // VTEX CLI Args
            string[] args =
            {
                $"\"{folderPath}\\{fileName}.tga\"",
                "-nopause",
                "-game",
                $"\"{tf2Path}\\tf\\\""
            };

            Process.Start($"{tf2Path}\\bin\\vtex.exe", string.Join(" ", args)).WaitForExit();
        }
    }
}