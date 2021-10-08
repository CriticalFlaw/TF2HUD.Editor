using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Json;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows;
using HUDEditor.Models;
using HUDEditor.Properties;
using Microsoft.Win32;
using WPFLocalizeExtension.Extensions;

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
            return control.ComboFiles;
        }

        /// <summary>
        ///     Check if Team Fortress 2 is currently running.
        /// </summary>
        /// <returns>False if there's no active process named hl2, otherwise return true and a warning message.</returns>
        public static bool CheckIsGameRunning()
        {
            if (!Process.GetProcessesByName("hl2").Any()) return false;
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
            var registry = (string)Registry.GetValue($@"HKEY_LOCAL_MACHINE\Software\{is64Bit}Valve\Steam", "InstallPath", null);

            if (string.IsNullOrWhiteSpace(registry)) return false;

            // Found the Steam library path, now try to find the TF2 path.
            registry += "\\steamapps\\common\\Team Fortress 2\\tf\\custom";

            if (!Directory.Exists(registry)) return false;
            MainWindow.Logger.Info("tf/custom directory found in the registry: " + registry);
            Settings.Default.hud_directory = registry;
            Settings.Default.Save();
            return true;
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
        public static string EncodeID(string id)
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
            using (HttpClient client = new())
            {
                client.DefaultRequestHeaders.Add("User-Agent", "request");
                return await client.GetFromJsonAsync<T>(url);
            }
        }

        /// <summary>
        ///     Downloads a file from URL to the specified file path.
        /// </summary>
        /// <param name="url">URL to request resource from.</param>
        /// <param name="filePath">Relative path to file to save resource to.</param>
        public static async Task DownloadFile(string url, string filePath)
        {
            using (HttpClient client = new())
            {
                client.DefaultRequestHeaders.Add("User-Agent", "request");
                File.WriteAllBytes(filePath, await client.GetByteArrayAsync(url));
            }
        }

        /// <summary>
        ///     Calculates a file hash identical to the output of <c>git hash-object &lt;file&gt;</c>
        /// </summary>
        /// <param name="filePath">Path to file to calculate hash of.</param>
        public static string GitHash(string filePath)
        {
            string contents = File.ReadAllText(filePath, System.Text.Encoding.UTF8).Replace("\r\n", "\n");
            string headerString = $"blob {contents.Length}\0{contents}";
            byte[] contentBytes = System.Text.Encoding.UTF8.GetBytes(headerString);
            byte[] hashedBytes = System.Security.Cryptography.SHA1.HashData(contentBytes);
            return hashedBytes.Aggregate("", (string a, byte b) => a + b.ToString("X2").ToLower());
        }
    }
}