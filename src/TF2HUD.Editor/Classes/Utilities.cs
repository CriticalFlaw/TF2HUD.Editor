using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Json;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;
using HUDEditor.Models;
using HUDEditor.Properties;
using Microsoft.Win32;
using WPFLocalizeExtension.Extensions;
using Color = System.Windows.Media.Color;

namespace HUDEditor.Classes
{
    public static class Utilities
    {
        public static readonly List<Tuple<string, string, string>> ItemRarities = new()
        {
            new Tuple<string, string, string>("QualityColorNormal", "DimmQualityColorNormal",
                "QualityColorNormal_GreyedOut"),
            new Tuple<string, string, string>("QualityColorUnique", "DimmQualityColorUnique",
                "QualityColorUnique_GreyedOut"),
            new Tuple<string, string, string>("QualityColorStrange", "DimmQualityColorStrange",
                "QualityColorStrange_GreyedOut"),
            new Tuple<string, string, string>("QualityColorVintage", "DimmQualityColorVintage",
                "QualityColorVintage_GreyedOut"),
            new Tuple<string, string, string>("QualityColorHaunted", "DimmQualityColorHaunted",
                "QualityColorHaunted_GreyedOut"),
            new Tuple<string, string, string>("QualityColorrarity1", "DimmQualityColorrarity1",
                "QualityColorrarity1_GreyedOut"),
            new Tuple<string, string, string>("QualityColorCollectors", "DimmQualityColorCollectors",
                "QualityColorCollectors_GreyedOut"),
            new Tuple<string, string, string>("QualityColorrarity4", "DimmQualityColorrarity4",
                "QualityColorrarity4_GreyedOut"),
            new Tuple<string, string, string>("QualityColorCommunity", "DimmQualityColorCommunity",
                "QualityColorCommunity_GreyedOut"),
            new Tuple<string, string, string>("QualityColorDeveloper", "DimmQualityColorDeveloper",
                "QualityColorDeveloper_GreyedOut"),
            new Tuple<string, string, string>("ItemRarityCommon", "DimmItemRarityCommon", "ItemRarityCommon_GreyedOut"),
            new Tuple<string, string, string>("ItemRarityUncommon", "DimmItemRarityUncommon",
                "ItemRarityUncommon_GreyedOut"),
            new Tuple<string, string, string>("ItemRarityRare", "DimmItemRarityRare", "ItemRarityRare_GreyedOut"),
            new Tuple<string, string, string>("ItemRarityMythical", "DimmItemRarityMythical",
                "ItemRarityMythical_GreyedOut"),
            new Tuple<string, string, string>("ItemRarityLegendary", "DimmItemRarityLegendary",
                "ItemRarityLegendary_GreyedOut"),
            new Tuple<string, string, string>("ItemRarityAncient", "DimmItemRarityAncient",
                "ItemRarityAncient_GreyedOut")
        };

        public static readonly List<string> CrosshairStyles = new()
        {
            "!", "#", "$", "%", "'", "(", ")", "*", "+", ",", ".", "1", "2", "3", "4", "5", "6", "7", "8", "9", ":",
            ";", "=", "<", ">", "?", "@", "|", "}", "`", "_", "^", "]", "[", "A", "B", "C", "D", "E", "F", "G", "H",
            "I", "J", "K", "L", "M", "N", "O", "P", "Q", "R", "S", "T", "U", "V", "W", "X", "Y", "Z", "a", "b", "c",
            "d", "e", "f", "g", "h", "i", "j", "k", "m", "n", "o", "p", "q", "r", "s", "t", "u", "v", "w", "x", "y", "z"
        };

        /// <summary>
        ///     Add a comment tag (//) to the beginning of a text line.
        /// </summary>
        /// <param name="lines">Text lines from a file to process.</param>
        /// <param name="index">Line number to which to add a comment tag.</param>
        public static string CommentTextLine(string[] lines, int index)
        {
            MainWindow.Logger.Info($"Commenting line: {index}");
            return string.Concat("//", lines[index].Replace("//", string.Empty));
        }

        /// <summary>
        ///     Remove all comment tags (//) from a text line.
        /// </summary>
        /// <param name="lines">Text lines from a file to process.</param>
        /// <param name="index">Line number from which to remove a comment tag.</param>
        public static string UncommentTextLine(string[] lines, int index)
        {
            MainWindow.Logger.Info($"Uncommenting line: {index}");
            return lines[index].Replace("//", string.Empty);
        }

        /// <summary>
        ///     Get a list of line numbers containing a given string.
        /// </summary>
        /// <param name="lines">An array of lines to loop through.</param>
        /// <param name="value">String value to look for in the list of lines.</param>
        public static List<int> GetLineNumbersContainingString(string[] lines, string value)
        {
            // Loop through each line in the array, add any line number containing the value parameter to the list.
            var indexList = new List<int>();
            for (var x = 0; x < lines.Length; x++)
                if (lines[x].Contains(value) || lines[x].Contains(value.Replace(" ", "\t")))
                    indexList.Add(x);
            return indexList;
        }

        /// <summary>
        ///     Convert a HEX color code to RGBA.
        /// </summary>
        /// <param name="hex">HEX color code to be convert to RGBA.</param>
        public static string ConvertToRgba(string hex)
        {
            var color = ColorTranslator.FromHtml(hex);
            MainWindow.Logger.Info($"Converting {hex} to {color}.");
            return $"{color.R} {color.G} {color.B} {color.A}";
        }

        /// <summary>
        ///     Get a pulsed color by reducing a color channel value by 50.
        /// </summary>
        /// <param name="rgba">RGBA color code to process.</param>
        public static string GetPulsedColor(string rgba)
        {
            // Apply the pulse change and return the color.
            var colors = Array.ConvertAll(rgba.Split(' '), int.Parse);
            colors[^1] = colors[^1] >= 50 ? colors[^1] - 50 : colors[^1];
            return $"{colors[0]} {colors[1]} {colors[2]} {colors[^1]}";
        }

        /// <summary>
        ///     Get a darkened color by reducing each color channel by 40%.
        /// </summary>
        /// <param name="rgba">RGBA color code to process.</param>
        public static string GetShadowColor(string rgba)
        {
            // Reduce each color channel (except alpha) by 40%, then return the color.
            var colors = Array.ConvertAll(rgba.Split(' '), int.Parse);
            for (var x = 0; x < colors.Length; x++)
                colors[x] = Convert.ToInt32(colors[x] * 0.60);
            return $"{colors[0]} {colors[1]} {colors[2]} 255";
        }

        /// <summary>
        ///     Get a dimmed color by setting the alpha channel to 100.
        /// </summary>
        /// <param name="rgba">RGBA color code to process.</param>
        public static string GetDimmedColor(string rgba)
        {
            // Return the color with a reduced alpha channel.
            var colors = Array.ConvertAll(rgba.Split(' '), int.Parse);
            return $"{colors[0]} {colors[1]} {colors[2]} 100";
        }

        /// <summary>
        ///     Get a grayed color by reducing each color channel by 75%.
        /// </summary>
        /// <param name="rgba">RGBA color code to process.</param>
        public static string GetGrayedColor(string rgba)
        {
            // Reduce each color channel (except alpha) by 75%, then return the color.
            var colors = Array.ConvertAll(rgba.Split(' '), int.Parse);
            for (var x = 0; x < colors.Length; x++)
                colors[x] = Convert.ToInt32(colors[x] * 0.25);
            return $"{colors[0]} {colors[1]} {colors[2]} 255";
        }

        /// <summary>
        ///     Convert an RGBA color code to Color object.
        /// </summary>
        /// <param name="rgba">RGBA color code to process.</param>
        public static Color ConvertToColor(string rgba)
        {
            var colors = Array.ConvertAll(rgba.Split(' '), byte.Parse);
            return Color.FromArgb(colors[^1], colors[0], colors[1], colors[2]);
        }

        /// <summary>
        ///     Convert an RGBA color code to ColorBrush object.
        /// </summary>
        /// <param name="rgba">RGBA color code to process.</param>
        public static SolidColorBrush ConvertToColorBrush(string rgba)
        {
            var colors = Array.ConvertAll(rgba.Split(' '), byte.Parse);
            return new SolidColorBrush(Color.FromArgb(colors[^1], colors[0], colors[1], colors[2]));
        }

        /// <summary>
        ///     Open the provided path in browser or Windows Explorer.
        /// </summary>
        /// <param name="url">URL link to open.</param>
        public static void OpenWebpage(string url)
        {
            if (string.IsNullOrWhiteSpace(url)) return;
            MainWindow.Logger.Info($"Opening URL: {url}");
            Process.Start("explorer", url);
        }

        /// <summary>
        ///     Get the filename from the HUD schema control using a string value.
        /// </summary>
        /// <param name="control">Schema control to retrieve file names from.</param>
        internal static dynamic GetFileNames(Controls control)
        {
            if (!string.IsNullOrWhiteSpace(control.FileName))
                return control.FileName.Replace(".res", string.Empty);
            return (control.ComboDirectories is not null) ? control.ComboDirectories : control.ComboFiles;
        }

        /// <summary>
        ///     Check if Team Fortress 2 is currently running.
        /// </summary>
        /// <returns>False if there's no active process named hl2, tf, or tf_win64, otherwise return true and a warning message.</returns>
        public static bool CheckIsGameRunning()
        {
            if (!Process.GetProcessesByName("hl2").Any() && !Process.GetProcessesByName("tf").Any() && !Process.GetProcessesByName("tf_win64").Any()) return false;
            MainWindow.ShowMessageBox(MessageBoxImage.Warning, GetLocalizedString("info_game_running"));
            return true;
        }

        /// <summary>
        ///     Search registry for the Team Fortress 2 installation directory.
        /// </summary>
        /// <returns>True if the TF2 directory was found through the registry, otherwise return False.</returns>
        public static bool SearchRegistry()
        {
            // Try to find the Steam library path in the registry.
            var is64Bit = Environment.Is64BitProcess ? "Wow6432Node\\" : string.Empty;
            var pathFile = (string)Registry.GetValue($@"HKEY_LOCAL_MACHINE\Software\{is64Bit}Valve\Steam", "InstallPath", null) + "\\steamapps\\libraryfolders.vdf";
            if (!File.Exists(pathFile)) return false;

            // Read the file and attempt to extract all library paths.
            using var reader = new StreamReader(pathFile);
            var steamPaths = new List<string>();
            foreach (Match match in Regex.Matches(reader.ReadToEnd(), "\"(.*)\"\t*\"(.*)\""))
            {
                if (match.Groups[1].Value.Equals("path"))
                    steamPaths.Add(match.Groups[2].Value);
            }

            // Loop through all known library paths to try and find TF2.
            foreach (var path in steamPaths)
            {
                var pathTF = path + "\\steamapps\\common\\Team Fortress 2\\tf\\custom";
                if (Directory.Exists(pathTF))
                {
                    MainWindow.Logger.Info($"tf/custom directory found in the registry: {pathTF}");
                    Settings.Default.hud_directory = pathTF;
                    Settings.Default.Save();
                    return true;
                }
            }
            return false;
        }

        /// <summary>
        ///     Check if the set tf/custom directory is valid.
        /// </summary>
        /// <returns>True if the set tf/custom directory is valid.</returns>
        public static bool CheckUserPath(string hudPath)
        {
            return !string.IsNullOrWhiteSpace(hudPath) && hudPath.EndsWith("tf\\custom");
        }

        /// <summary>
        ///     Get a localized string from the resource file.
        /// </summary>
        public static string GetLocalizedString(string key)
        {
            _ = new LocExtension(key).ResolveLocalizedValue(out string uiString);
            return uiString;
        }

        /// <summary>
        ///     Converts a HUD/control name into a WPF usable ID
        /// </summary>
        /// <param name="id">ID to sanitize.</param>
        /// <remarks>
        ///     If first character is a digit, add an underscore, then replace all dashes and whitespace characters with underscores.
        /// </remarks>
        public static string EncodeId(string id)
        {
            // If first character is a digit, add an underscore, then replace all dashes and whitespace characters with underscores
            return $"{(Regex.IsMatch(id[0].ToString(), "\\d") ? "_" : "")}{string.Join('_', Regex.Split(id, "[- ]"))}";
        }

        /// <summary>
        ///     Deep merge keys from one object to another
        /// </summary>
        /// <param name="object1"></param>
        /// <param name="object2"></param>
        public static void Merge(Dictionary<string, dynamic> object1, Dictionary<string, dynamic> object2)
        {
            try
            {
                foreach (var key in object1.Keys)
                    if (object1[key].GetType() == typeof(Dictionary<string, dynamic>))
                    {
                        if (object2.ContainsKey(key) && object2[key].GetType() == typeof(Dictionary<string, dynamic>))
                            Merge(object1[key], object2[key]);
                    }
                    else
                    {
                        if (object2.ContainsKey(key))
                            object1[key] = object2[key];
                    }

                foreach (var key in object2.Keys.Where(key => !object1.ContainsKey(key)))
                    object1[key] = object2[key];
            }
            catch (Exception e)
            {
                MainWindow.Logger.Error(e);
                Console.WriteLine(e);
                throw;
            }
        }

        /// <summary>
        ///     Adds the structure for empty nested objects inside a given object
        /// </summary>
        /// <param name="obj">Object to add nested objects to.</param>
        /// <param name="keys">Keys to add.</param>
        public static Dictionary<string, dynamic> CreateNestedObject(Dictionary<string, dynamic> obj, IEnumerable<string> keys)
        {
            try
            {
                var objectRef = obj;
                foreach (var key in keys)
                {
                    if (!objectRef.ContainsKey(key))
                        objectRef[key] = new Dictionary<string, dynamic>();
                    objectRef = objectRef[key];
                }

                return objectRef;
            }
            catch (Exception e)
            {
                MainWindow.Logger.Error(e);
                Console.WriteLine(e);
                throw;
            }
        }

        /// <summary>
        ///     Fetch JSON from specified URL.
        /// </summary>
        /// <param name="url">URL to request resource from.</param>
        public static async Task<T> Fetch<T>(string url)
        {
            using HttpClient client = new();
            client.DefaultRequestHeaders.Add("User-Agent", "request");
            return await client.GetFromJsonAsync<T>(url);
        }

        /// <summary>
        ///     Downloads a file from URL to the specified file path.
        /// </summary>
        /// <param name="url">URL to request resource from.</param>
        /// <param name="filePath">Relative path to file to save resource to.</param>
        public static async Task DownloadFile(string url, string filePath)
        {
            using HttpClient client = new();
            client.DefaultRequestHeaders.Add("User-Agent", "request");
            File.WriteAllBytes(filePath, await client.GetByteArrayAsync(url));
        }

        /// <summary>
        ///     Calculates a file hash identical to the output of <c>git hash-object &lt;file&gt;</c>
        /// </summary>
        /// <param name="filePath">Path to file to calculate hash of.</param>
        public static string GitHash(string filePath)
        {
            var contents = File.ReadAllText(filePath, System.Text.Encoding.UTF8).Replace("\r\n", "\n");
            var headerString = $"blob {contents.Length}\0{contents}";
            var contentBytes = System.Text.Encoding.UTF8.GetBytes(headerString);
            var hashedBytes = System.Security.Cryptography.SHA1.HashData(contentBytes);
            return hashedBytes.Aggregate("", (a, b) => a + b.ToString("X2").ToLower());
        }

        /// <summary>
        ///     Install the Hypnotize TF2 HUD Crosshairs to a given HUD folder
        /// </summary>
        /// <param name="folderPath">Absolute folder path to HUD to install crosshairs to</param>
        public static async Task InstallCrosshairs(string folderPath)
        {
            const string crosshairsName = "TF2-HUD-Crosshairs-master";
            var crosshairsZipFileName = $"{crosshairsName}.zip";

            // Download TF2 HUD Crosshairs
            await Utilities.DownloadFile(Settings.Default.tf2_hud_crosshairs_zip, crosshairsZipFileName);
            if (Directory.Exists(crosshairsName)) Directory.Delete(crosshairsName, true);
            ZipFile.ExtractToDirectory(crosshairsZipFileName, folderPath);

            // Move crosshairs folder to HUD
            string targetDirectory = Path.Join(folderPath, "resource\\crosshairs");
            if (Directory.Exists(targetDirectory)) Directory.Delete(targetDirectory, true);
            Directory.Move(Path.Join(folderPath, Path.Join(crosshairsName, "crosshairs")), targetDirectory);
            Directory.Delete(Path.Join(folderPath, crosshairsName), true);

            async Task AddBaseReference(string relativeFilePath, string baseFilePath)
            {
                var absoluteFilePath = Path.Join(folderPath, relativeFilePath);

                // Assume absoluteFilePath exists in the HUD
                var obj = VDF.Parse(await File.ReadAllTextAsync(absoluteFilePath));

                if (!obj.ContainsKey("#base"))
                {
                    obj["#base"] = baseFilePath;
                }
                else
                {
                    var baseType = obj["#base"].GetType();
                    if (baseType == typeof(string))
                    {
                        obj["#base"] = new List<dynamic> { (string)obj["#base"], baseFilePath };
                    }
                    else if (baseType == typeof(List<dynamic>))
                    {
                        obj["#base"].Add(baseFilePath);
                    }
                    else
                    {
                        throw new Exception($"Unexpected #base value type in {relativeFilePath}. Expected string or list");
                    }
                }

                await File.WriteAllTextAsync(absoluteFilePath, VDF.Stringify(obj));
            }

            await Task.WhenAll(
                // Add #base statements to HUD files as per https://github.com/Hypnootize/TF2-HUD-Crosshairs#installation
                AddBaseReference("resource\\clientscheme.res", "../resource/crosshairs/crosshair_scheme.res"),
                AddBaseReference("scripts\\hudlayout.res", "../resource/crosshairs/crosshair.res"),

                // Add "file" reference to hudanimations_manifest.txt
                Task.Run(async () =>
                {
                    var filePath = Path.Join(folderPath, "scripts\\hudanimations_manifest.txt");

                    // If the HUD does not contain a hudanimations_manifest.txt,
                    // use the string from tf2_misc_dir.vpk scripts/hudanimations_manifest.txt
                    var fileContents = File.Exists(filePath)
                    ? await File.ReadAllTextAsync(filePath)
                    : @"
                    hudanimations_manifest
                    {
                        file scripts/hudanimations.txt
                        file scripts/hudanimations_tf.txt
                    }
                    ";

                    var hudAnimationsManifest = VDF.Parse(fileContents);
                    List<dynamic> files = hudAnimationsManifest["hudanimations_manifest"]["file"];

                    const string animationsBasePath = "resource/crosshairs/crosshair_animation.txt";

                    if (!files.Contains(animationsBasePath))
                        files.Insert(0, animationsBasePath);

                    await File.WriteAllTextAsync(filePath, VDF.Stringify(hudAnimationsManifest));
                })
            );
        }
    }
}