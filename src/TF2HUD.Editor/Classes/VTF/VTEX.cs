using log4net;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HUDEditor.Classes
{
    public class VTEX
    {
        private readonly ILog _logger;

        public VTEX(ILog logger)
        {
            _logger = logger;
        }

        /// <summary>
        ///     Use the Valve Texture Tool (Vtex) to convert a Targa file (tga) into a Valve Texture File (vtf).
        /// </summary>
        /// <param name="folderPath">Input image file path.</param>
        /// <param name="fileName">Name of the image to be converted.</param>
        /// <returns>Path to the generated vtf file.</returns>
        /// <remarks>See: https://developer.valvesoftware.com/wiki/Vtex_compile_parameters </remarks>
        public string Convert(string tf2Path, string folderPath, string fileName)
        {
            SaveCompileParameters(folderPath, fileName);
            var process = RunVTEX(tf2Path, folderPath, fileName);
            LogOutput(process);
            Close(process);

            return $"{tf2Path}\\tf\\materials\\{fileName}.vtf";
        }

        private static void SaveCompileParameters(string folderPath, string fileName)
        {
            var compileParameters = new[]
            {
                "pointsample 1",
                "nolod 1",
                "nomip 1"
            };
            var compileParametersFilePath = $"{folderPath}\\{fileName}.txt";
            File.WriteAllLines(compileParametersFilePath, compileParameters);
        }

        private Process RunVTEX(string tf2Path, string folderPath, string fileName)
        {
            string[] vtexCliArgs =
                        {
                "-quiet",
                $"\"{folderPath}\\{fileName}.tga\""
            };

            var vtexPath = $"{tf2Path}\\bin\\vtex.exe";
            var processInfo = new ProcessStartInfo(vtexPath)
            {
                Arguments = string.Join(" ", vtexCliArgs),
                RedirectStandardOutput = true
            };
            var process = Process.Start(processInfo);
            return process;
        }

        private void LogOutput(Process process)
        {
            while (!process.StandardOutput.EndOfStream)
                _logger.Info($"[VTEX] {process.StandardOutput.ReadLine()}");
        }

        private static void Close(Process process)
        {
            process?.WaitForExit();
            process?.Close();
        }
    }
}
