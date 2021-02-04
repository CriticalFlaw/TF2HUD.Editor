using System;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using TF2HUD.Editor.Properties;

namespace TF2HUD.Editor.Common
{
    internal class VTF
    {
        private readonly string TF2Path;

        public VTF(string TF2Path)
        {
            this.TF2Path = TF2Path.Replace("\\tf\\custom", string.Empty);
        }

        public void Convert(string InFile, string OutFile)
        {
            // Resize image to square of larger proportional
            var Image = new Bitmap(InFile);
            var SquareImage = ResizeImage(Image);

            var MateralSrc = $"{TF2Path}\\tf\\materialsrc";

            // Create materialsrc (ensure it exists)
            Directory.CreateDirectory(MateralSrc);

            // Save image as .tga (cast using TGASharpLib)
            var TGASquareImage = (TGA) SquareImage;
            TGASquareImage.Save($"{MateralSrc}\\temp.tga");

            // Convert using VTEX
            VTEXConvert(MateralSrc, "temp");

            // Path to VTEX output file
            var VTFLocation = $"{TF2Path}\\tf\\materials\\temp.vtf";

            // Create absolute path to output folder and make directory
            var PathInfo = OutFile.Split('\\', '/');
            PathInfo[PathInfo.Length - 1] = "";
            var FolderPath = string.Join("\\", PathInfo);
            Directory.CreateDirectory(FolderPath);

            // Make a backup of the existing background files.
            var BgPath =
                new DirectoryInfo(string.Format(Resources.path_console, MainWindow.HudPath, MainWindow.HudSelection));
            foreach (var file in BgPath.GetFiles())
                File.Delete(file.FullName);

            // Copy vtf from vtex output to user defined path
            File.Copy(VTFLocation, OutFile, true);

            // Delete temporary tga and vtex output
            File.Delete($"{MateralSrc}\\temp.tga");
            File.Delete($"{MateralSrc}\\temp.txt");
            File.Delete(VTFLocation);
        }

        private Bitmap ResizeImage(Bitmap Image)
        {
            var ImageMaxSize = Math.Max(Image.Width, Image.Height);
            var Power = 2;
            do
            {
                Power *= 2;
            } while (Power < ImageMaxSize);

            // Paint G onto SquareImage Bitmap
            var SquareImage = new Bitmap(Power, Power);
            var G = Graphics.FromImage(SquareImage);
            G.DrawImage(Image, 0, 0, Power, Power);

            return SquareImage;
        }

        private void VTEXConvert(string FolderPath, string FileNameNoExt)
        {
            // VTEX Args
            // https://developer.valvesoftware.com/wiki/Vtex_compile_parameters
            File.WriteAllLines($"{FolderPath}\\{FileNameNoExt}.txt", new[]
            {
                "pointsample 1",
                "nolod 1",
                "nomip 1"
            });

            // VTEX CLI Args
            string[] Args =
            {
                $"\"{FolderPath}\\{FileNameNoExt}.tga\"",
                "-nopause",
                "-game",
                $"\"{TF2Path}\\tf\\\""
            };

            var ArgsString = string.Join(" ", Args);
            Process.Start($"{TF2Path}\\bin\\vtex.exe", ArgsString).WaitForExit();
        }
    }
}