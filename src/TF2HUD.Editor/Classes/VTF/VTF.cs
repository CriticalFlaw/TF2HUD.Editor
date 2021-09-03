using log4net;
using System;
using System.Diagnostics;
using System.Drawing;
using System.IO;

namespace HUDEditor.Classes
{
    public class VTF
    {
        private readonly VTEX _vtex;
        private static string MaterialSrcDirectory(string tf2Path) => $"{tf2Path}\\tf\\materialsrc";

        public VTF(VTEX vtex)
        {
            _vtex = vtex;
        }

        /// <summary>
        ///     Convert an image file into a VTF texture file to be used in-game.
        /// </summary>
        /// <param name="inFile">Path and file name of the image to be converted.</param>
        /// <param name="outFile">Output path and file name for the VTF.</param>
        public void Convert(string tf2Path, string inFile, string outFile)
        {
            var workingFileName = "temp";
            var materialSrc = MaterialSrcDirectory(tf2Path);

            CreateTGAFile(inFile, workingFileName);
            var vtfFilePath = CreateVTFFile(tf2Path, workingFileName, materialSrc);

            DeleteExistingBackgroundFiles();

            SaveVTF(vtfFilePath, outFile);

            DeleteWorkingFiles(materialSrc, vtfFilePath, workingFileName);
        }

        private string CreateVTFFile(string tf2Path, string workingFileName, string materialSrc)
        {
            return _vtex.Convert(tf2Path, materialSrc, workingFileName);
        }

        private void CreateTGAFile(string inFile, string workingFileName)
        {
            var tgaImage = CreateTGAImage(inFile);
            SaveTGA(tgaImage, inFile, workingFileName);
        }

        /// <summary>
        /// Copy vtf from vtex output to user defined path
        /// </summary>
        /// <param name="outFile"></param>
        /// <param name="vtfFilePath"></param>
        private static void SaveVTF(string vtfFilePath, string outFile)
        {
            Directory.CreateDirectory(Path.GetDirectoryName(outFile));
            File.Copy(vtfFilePath, outFile, true);
        }

        /// <summary>
        /// Delete temporary tga and vtex output
        /// </summary>
        /// <param name="materialSrc"></param>
        /// <param name="vtfOutput"></param>
        private static void DeleteWorkingFiles(string materialSrc, string vtfOutput, string fileName)
        {
            File.Delete($"{materialSrc}\\{fileName}.tga");
            File.Delete($"{materialSrc}\\{fileName}.txt");
            File.Delete(vtfOutput);
        }

        private static void DeleteExistingBackgroundFiles()
        {
            var hudBgPath = new DirectoryInfo($"{MainWindow.HudPath}\\{MainWindow.HudSelection}\\materials\\console\\");
            foreach (var file in hudBgPath.GetFiles())
                File.Delete(file.FullName);
        }

        /// <summary>
        /// Save TGA image to disk.
        /// </summary>
        /// <param name="image">TGA image to save.</param>
        /// <param name="materialSrc">Directory of tf\materialsrc.</param>
        /// <param name="fileName">Name of the saved TGA file.</param>
        /// <returns>Path to TGA file.</returns>
        private string SaveTGA(TGA image, string materialSrc, string fileName)
        {
            var tgaFilePath = $"{materialSrc}\\{fileName}.tga";
            Directory.CreateDirectory(materialSrc);
            image.Save(tgaFilePath);

            return tgaFilePath;
        }

        /// <summary>
        /// Converts image to .tga (cast using TGASharpLib)
        /// </summary>
        /// <param name="inFile"></param>
        /// <returns></returns>
        private static TGA CreateTGAImage(string inFile)
        {
            var image = new Bitmap(inFile);
            var tgaSquareImage = (TGA)ResizeImage(image);
            return tgaSquareImage;
        }

        /// <summary>
        ///     Resize the image file so it can be saved into Targa file.
        /// </summary>
        /// <param name="image">Bitmap of the input image file.</param>
        /// <returns>Bitmap of the resized image file.</returns>
        private static Bitmap ResizeImage(Bitmap image)
        {
            var size = GetImageSize(image);
            return DrawImage(image, size);
        }

        /// <summary>
        /// Paint graphics onto the SquareImage Bitmap
        /// </summary>
        /// <param name="image"></param>
        /// <param name="size"></param>
        /// <returns></returns>
        private static Bitmap DrawImage(Bitmap image, int size)
        {
            var squareImage = new Bitmap(size, size);
            var graphics = Graphics.FromImage(squareImage);
            graphics.DrawImage(image, 0, 0, size, size);
            return squareImage;
        }

        /// <summary>
        /// Image size is the greater of both the width and height rounded up to the nearest power of 2
        /// </summary>
        /// <param name="image"></param>
        /// <returns></returns>
        private static int GetImageSize(Bitmap image)
        {
            var width = Math.Pow(2, Math.Ceiling(Math.Log(image.Width) / Math.Log(2)));
            var height = Math.Pow(2, Math.Ceiling(Math.Log(image.Height) / Math.Log(2)));
            return (int)Math.Max(width, height);
        }
    }
}