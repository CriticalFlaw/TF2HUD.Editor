using Avalonia.Media;
using Avalonia.Platform;
using Avalonia.Platform.Storage;
using HUDEdit.Assets;
using HUDEdit.Models;
using HUDEdit.Views;
using Microsoft.Win32;
using MsBox.Avalonia;
using MsBox.Avalonia.Enums;
using Octokit;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Json;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Color = Avalonia.Media.Color;

namespace HUDEdit.Classes;

public static class Utilities
{
    public static readonly List<Tuple<string, string, string>> ItemRarities =
    [
        new Tuple<string, string, string>("QualityColorNormal", "DimmQualityColorNormal", "QualityColorNormal_GreyedOut"),
        new Tuple<string, string, string>("QualityColorUnique", "DimmQualityColorUnique", "QualityColorUnique_GreyedOut"),
        new Tuple<string, string, string>("QualityColorStrange", "DimmQualityColorStrange", "QualityColorStrange_GreyedOut"),
        new Tuple<string, string, string>("QualityColorVintage", "DimmQualityColorVintage", "QualityColorVintage_GreyedOut"),
        new Tuple<string, string, string>("QualityColorHaunted", "DimmQualityColorHaunted", "QualityColorHaunted_GreyedOut"),
        new Tuple<string, string, string>("QualityColorrarity1", "DimmQualityColorrarity1", "QualityColorrarity1_GreyedOut"),
        new Tuple<string, string, string>("QualityColorCollectors", "DimmQualityColorCollectors", "QualityColorCollectors_GreyedOut"),
        new Tuple<string, string, string>("QualityColorrarity4", "DimmQualityColorrarity4", "QualityColorrarity4_GreyedOut"),
        new Tuple<string, string, string>("QualityColorCommunity", "DimmQualityColorCommunity", "QualityColorCommunity_GreyedOut"),
        new Tuple<string, string, string>("QualityColorDeveloper", "DimmQualityColorDeveloper", "QualityColorDeveloper_GreyedOut"),
        new Tuple<string, string, string>("ItemRarityCommon", "DimmItemRarityCommon", "ItemRarityCommon_GreyedOut"),
        new Tuple<string, string, string>("ItemRarityUncommon", "DimmItemRarityUncommon", "ItemRarityUncommon_GreyedOut"),
        new Tuple<string, string, string>("ItemRarityRare", "DimmItemRarityRare", "ItemRarityRare_GreyedOut"),
        new Tuple<string, string, string>("ItemRarityMythical", "DimmItemRarityMythical", "ItemRarityMythical_GreyedOut"),
        new Tuple<string, string, string>("ItemRarityLegendary", "DimmItemRarityLegendary", "ItemRarityLegendary_GreyedOut"),
        new Tuple<string, string, string>("ItemRarityAncient", "DimmItemRarityAncient", "ItemRarityAncient_GreyedOut")
    ];

    public static readonly List<string> CrosshairStyles =
    [
        "!", "#", "$", "%", "'", "(", ")", "*", "+", ",", ".", "1", "2", "3", "4", "5", "6", "7", "8", "9", ":",
        ";", "=", "<", ">", "?", "@", "|", "}", "`", "_", "^", "]", "[", "A", "B", "C", "D", "E", "F", "G", "H",
        "I", "J", "K", "L", "M", "N", "O", "P", "Q", "R", "S", "T", "U", "V", "W", "X", "Y", "Z", "a", "b", "c",
        "d", "e", "f", "g", "h", "i", "j", "k", "m", "n", "o", "p", "q", "r", "s", "t", "u", "v", "w", "x", "y", "z"
    ];

    /// <summary>
    /// Adds a comment tag (//) to the beginning of a text line.
    /// </summary>
    /// <param name="lines">Text lines from a file to process.</param>
    /// <param name="index">Line number to which to add a comment tag.</param>
    public static string CommentTextLine(string[] lines, int index)
    {
        App.Logger.Info($"Commenting line {index}");
        return string.Concat("//", lines[index].Replace("//", string.Empty));
    }

    /// <summary>
    /// Removes all comment tags (//) from a text line.
    /// </summary>
    /// <param name="lines">Text lines from a file to process.</param>
    /// <param name="index">Line number from which to remove a comment tag.</param>
    public static string UncommentTextLine(string[] lines, int index)
    {
        App.Logger.Info($"Uncommenting line {index}");
        return lines[index].Replace("//", string.Empty);
    }

    /// <summary>
    /// Gets a list of line numbers containing a given string.
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
    /// Converts a HEX color code to RGBA.
    /// </summary>
    /// <param name="hex">HEX color code to be convert to RGBA.</param>
    public static string ConvertToRgba(string hex)
    {
        var color = ColorTranslator.FromHtml(hex);
        App.Logger.Info($"Converting {hex} to {color}");
        return $"{color.R} {color.G} {color.B} {color.A}";
    }

    /// <summary>
    /// Gets a pulsed color by reducing a color channel value by 50.
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
    /// Gets a darkened color by reducing each color channel by 40%.
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
    /// Gets a dimmed color by setting the alpha channel to 100.
    /// </summary>
    /// <param name="rgba">RGBA color code to process.</param>
    public static string GetDimmedColor(string rgba)
    {
        // Return the color with a reduced alpha channel.
        var colors = Array.ConvertAll(rgba.Split(' '), int.Parse);
        return $"{colors[0]} {colors[1]} {colors[2]} 100";
    }

    /// <summary>
    /// Gets a grayed color by reducing each color channel by 75%.
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
    /// Converts an RGBA color code to Color object.
    /// </summary>
    /// <param name="rgba">RGBA color code to process.</param>
    public static Color ConvertToColor(string rgba)
    {
        var colors = Array.ConvertAll(rgba.Split(' '), byte.Parse);
        return Color.FromArgb(colors[^1], colors[0], colors[1], colors[2]);
    }

    /// <summary>
    /// Converts an RGBA color code to ColorBrush object.
    /// </summary>
    /// <param name="rgba">RGBA color code to process.</param>
    public static SolidColorBrush ConvertToColorBrush(string rgba)
    {
        var colors = Array.ConvertAll(rgba.Split(' '), byte.Parse);
        return new SolidColorBrush(Color.FromArgb(colors[^1], colors[0], colors[1], colors[2]));
    }

    /// <summary>
    /// Opens the provided path in browser or Windows Explorer.
    /// </summary>
    /// <param name="url">URL link to open.</param>
    public static void OpenWebpage(string url)
    {
        if (string.IsNullOrWhiteSpace(url)) return;
        App.Logger.Info($"Opening URL: {url}");
        Process.Start(RuntimeInformation.IsOSPlatform(OSPlatform.Linux) ? "xdg-open" : "explorer", url);
    }

    /// <summary>
    /// Gets the filename from the HUD schema control using a string value.
    /// </summary>
    /// <param name="control">Schema control to retrieve file names from.</param>
    internal static dynamic GetFileNames(Controls control)
    {
        if (!string.IsNullOrWhiteSpace(control.FileName))
            return control.FileName.Replace(".res", string.Empty);
        return (control.ComboDirectories is not null) ? control.ComboDirectories : control.ComboFiles;
    }

    /// <summary>
    /// Checks if Team Fortress 2 is currently running.
    /// </summary>
    /// <returns>False if there's no active process named hl2, tf, or tf_win64, otherwise return true and a warning message.</returns>
    public static bool CheckIsGameRunning()
    {
        if (Process.GetProcessesByName("hl2").Length == 0 && Process.GetProcessesByName("tf").Length == 0 && Process.GetProcessesByName("tf_win64").Length == 0) return false;
        ShowMessageBox(Resources.info_game_running, MsBox.Avalonia.Enums.Icon.Warning);
        return true;
    }

    /// <summary>
    /// Searchs registry for the Team Fortress 2 installation directory.
    /// </summary>
    /// <returns>True if the TF2 directory was found through the registry, otherwise return False.</returns>
    public static bool SearchRegistry()
    {
        var steamPaths = new List<string>();

        // Do not bother searching the registry if on Linux.
        if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux)) return false;

        var is64Bit = Environment.Is64BitProcess ? "Wow6432Node/" : string.Empty;
        var pathFile = (string)Registry.GetValue($@"HKEY_LOCAL_MACHINE\Software\{is64Bit}Valve\Steam", "InstallPath", null) + "/steamapps/libraryfolders.vdf";
        if (!File.Exists(pathFile)) return false;

        // Read the file and attempt to extract all library paths.
        using var reader = new StreamReader(pathFile);
        foreach (Match match in Regex.Matches(reader.ReadToEnd(), "\"(.*)\"\t*\"(.*)\""))
        {
            if (match.Groups[1].Value.Equals("path"))
                steamPaths.Add(match.Groups[2].Value);
        }

        // Loop through all known library paths to try and find TF2.
        foreach (var path in steamPaths)
        {
            var pathTF = path + "/steamapps/common/Team Fortress 2/tf/custom";
            if (Directory.Exists(pathTF))
            {
                App.Logger.Info($"Set target directory to: {pathTF}");
                App.Config.ConfigSettings.UserPrefs.HUDDirectory = pathTF;
                App.SaveConfiguration();
                return true;
            }
        }
        return false;
    }

    /// <summary>
    /// Checks if the target directory is valid.
    /// </summary>
    /// <returns>True if the set target directory is valid.</returns>
    public static bool CheckUserPath()
    {
        return !string.IsNullOrWhiteSpace(App.HudPath) && (App.Config.ConfigSettings.UserPrefs.PathBypass || App.HudPath.EndsWith("tf/custom"));
    }

    /// <summary>
    /// Converts a HUD/control name into a WPF usable ID
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
    /// Deep merges keys from one object to another
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
            App.Logger.Error(e);
            Console.WriteLine(e);
            throw;
        }
    }

    /// <summary>
    /// Adds the structure for empty nested objects inside a given object
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
            App.Logger.Error(e);
            Console.WriteLine(e);
            throw;
        }
    }

    /// <summary>
    /// Fetches JSON from specified URL.
    /// </summary>
    /// <param name="url">URL to request resource from.</param>
    public static async Task<T> Fetch<T>(string url)
    {
        using HttpClient client = new();
        client.DefaultRequestHeaders.Add("User-Agent", "request");
        return await client.GetFromJsonAsync<T>(url);
    }

    /// <summary>
    /// Downloads a file from URL to the specified file path.
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
    /// Downloads and prepares the HUD for use.
    /// </summary>
    /// <param name="url">Download link for the HUD</param>
    /// <param name="filePath">Path to where the HUD should be installed to</param>
    /// <param name="hudName">Proper name for the HUD being downloaded</param>
    public static async Task DownloadHud(string url, string filePath, string hudName)
    {
        HttpClient client = new();
        client.DefaultRequestHeaders.Add("User-Agent", "request");

        App.Logger.Info($"Downloading {hudName} from {url}");
        var uri = new Uri(url);
        var bytes = uri.Scheme == "file"
            ? await File.ReadAllBytesAsync(uri.AbsolutePath)
            : await client.GetByteArrayAsync(uri);

        if (bytes.Length == 0)
        {
            // GameBanana returns 200 with an empty response for missing download links.
            throw new HttpRequestException($"Response from download source did not return a valid zip file");
        }

        // Create new ZIP object from bytes.
        var stream = new MemoryStream(bytes);
        var archive = new ZipArchive(stream);

        // Zip files made with ZipFile.CreateFromDirectory do not include directory entries, so create root directory*
        Directory.CreateDirectory($"{filePath}/{hudName}");

        foreach (var entry in archive.Entries)
        {
            // Remove first folder name from entry.FullName e.g. "flawhud-master" => "".
            var path = String.Join('/', entry.FullName.Split("/")[1..]);

            // Ignore directory entries
            // path == "" is root directory entry
            if (path != "" && !path.EndsWith('/'))
            {
                // *and ensure directory exists for each file
                Directory.CreateDirectory($"{filePath}/{hudName}/{Path.GetDirectoryName(path)}");
                entry.ExtractToFile($"{filePath}/{hudName}/{path}");
            }
        }

        // Clean the application directory.
        archive.Dispose();
    }

    /// <summary>
    /// Calculates a file hash identical to the output of <c>git hash-object &lt;file&gt;</c>
    /// </summary>
    /// <param name="filePath">Path to file to calculate hash of.</param>
    public static string GitHash(string filePath)
    {
        var contentBytes = File.ReadAllBytes(filePath);
        var header = $"blob {contentBytes.Length}\0";
        var headerBytes = System.Text.Encoding.UTF8.GetBytes(header);

        // Combine header and content
        var full = new byte[headerBytes.Length + contentBytes.Length];
        Buffer.BlockCopy(headerBytes, 0, full, 0, headerBytes.Length);
        Buffer.BlockCopy(contentBytes, 0, full, headerBytes.Length, contentBytes.Length);

        // Compute hash
        var hashedBytes = System.Security.Cryptography.SHA1.HashData(full);
        return BitConverter.ToString(hashedBytes).Replace("-", "").ToLowerInvariant();

    }

    /// <summary>
    /// Installs Hypnotize's TF2 HUD Crosshairs to a given HUD folder
    /// </summary>
    /// <param name="folderPath">Absolute folder path to HUD to install crosshairs to</param>
    public static async Task InstallCrosshairs(string folderPath)
    {
        const string crosshairsName = "TF2-HUD-Crosshairs-master";
        var crosshairsZipFileName = $"{crosshairsName}.zip";

        // Download TF2 HUD Crosshairs
        await DownloadFile(App.Config.ConfigSettings.AppConfig.CrosshairPackURL, crosshairsZipFileName);
        if (Directory.Exists(crosshairsName)) Directory.Delete(crosshairsName, true);
        ZipFile.ExtractToDirectory(crosshairsZipFileName, folderPath);

        // Move crosshairs folder to HUD
        string targetDirectory = Path.Join(folderPath, "resource/crosshairs");
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
            AddBaseReference("resource/clientscheme.res", "../resource/crosshairs/crosshair_scheme.res"),
            AddBaseReference("scripts/hudlayout.res", "../resource/crosshairs/crosshair.res"),

            // Add "file" reference to hudanimations_manifest.txt
            Task.Run(async () =>
            {
                var filePath = Path.Join(folderPath, "scripts/hudanimations_manifest.txt");

                // If the HUD does not contain a hudanimations_manifest.txt, use the string from tf2_misc_dir.vpk scripts/hudanimations_manifest.txt
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

    public static async Task InstallCastingEssentials()
    {
        await DownloadFile(App.Config.ConfigSettings.AppConfig.MastercomfigVpkURL, $"{App.HudPath}/CastingEssentialsNext");
    }

    public static Avalonia.Media.Imaging.Bitmap? LoadImage(string url)
    {
        try
        {
            using var httpClient = new HttpClient();
            using var response = httpClient.GetAsync(url).Result;

            if (!response.IsSuccessStatusCode)
                return null;

            using var stream = response.Content.ReadAsStreamAsync().Result;
            return new Avalonia.Media.Imaging.Bitmap(stream);
        }
        catch (Exception e)
        {
            Console.WriteLine($"Error downloading image: {e.Message}");
            return null;
        }
    }

    public static async Task<Avalonia.Media.Imaging.Bitmap?> LoadImageAsync(string url)
    {
        try
        {
            using HttpClient client = new();
            var response = await client.GetAsync(url);
            response.EnsureSuccessStatusCode();

            await using var stream = await response.Content.ReadAsStreamAsync();
            return new Avalonia.Media.Imaging.Bitmap(stream);
        }
        catch (Exception e)
        {
            Console.WriteLine($"Error loading image: {e.Message}");
            return null;
        }
    }

    public static Avalonia.Media.Imaging.Bitmap LoadFromResource(Uri resourceUri)
    {
        return new Avalonia.Media.Imaging.Bitmap(AssetLoader.Open(resourceUri));
    }

    public static void RestartApplication()
    {
        // TODO: Update for Linux
        try
        {
            // Get the current application's executable path
            var processStartInfo = new ProcessStartInfo
            {
                FileName = Environment.ProcessPath ?? AppContext.BaseDirectory,
                UseShellExecute = true
            };

            // Start a new instance
            Process.Start(processStartInfo);

            // Close the current application
            Environment.Exit(0);
        }
        catch (Exception e)
        {
            Console.WriteLine($"Failed to restart application: {e.Message}");
        }
    }

    /// <summary>
    /// Displays a set type of message box to the user.
    /// </summary>
    public static async Task ShowMessageBox(string message, MsBox.Avalonia.Enums.Icon type = MsBox.Avalonia.Enums.Icon.None)
    {
        switch (type)
        {
            case MsBox.Avalonia.Enums.Icon.Error:
                App.Logger.Error(message);
                break;

            case MsBox.Avalonia.Enums.Icon.Warning:
                App.Logger.Warn(message);
                break;
        }

        await MessageBoxManager.GetMessageBoxStandard(title: "Notice!", text: message, ButtonEnum.Ok, icon: type).ShowAsync();
    }

    /// <summary>
    /// Displays a message box asking for user input.
    /// </summary>
    /// <returns>
    /// TRUE if user clicks Yes, otherwise FALSE.
    /// </returns>
    public static async Task<ButtonResult> ShowPromptBox(string message)
    {
        return await MessageBoxManager.GetMessageBoxStandard(title: "Input Required!", text: message, ButtonEnum.YesNo, icon: MsBox.Avalonia.Enums.Icon.Question).ShowAsync();
    }

    /// <summary>
    /// Checks if the selected HUD is installed correctly.
    /// </summary>
    /// <returns>True if the selected hud is installed.</returns>
    public static bool CheckHudInstallation(HUD hud)
    {
        return hud != null &&
            App.HudPath != null &&
            CheckUserPath() &&
            Directory.Exists(App.HudPath) &&
            Directory.Exists($"{App.HudPath}/{hud.Name}");
    }

    /// <summary>
    /// Sets up the target HUD installation directory.
    /// </summary>
    /// <param name="userSet">If true, will prompt the user to provide the target directory.</param>
    public static async Task<bool> SetupDirectoryAsync(Avalonia.Controls.Window mainWindow, bool userSet = false)
    {
        if ((SearchRegistry() || CheckUserPath()) && !userSet) return true;

        App.Logger.Info("Target directory not set. Asking user to provide it.");
        var folders = await mainWindow.StorageProvider.OpenFolderPickerAsync(new FolderPickerOpenOptions
        {
            Title = Resources.info_path_browser,
            AllowMultiple = false
        });

        if (folders.Count > 0)
        {
            var userPath = folders[0].TryGetLocalPath().Replace("\\", "/");
            if (App.Config.ConfigSettings.UserPrefs.PathBypass || userPath.EndsWith("tf/custom"))
            {
                App.Config.ConfigSettings.UserPrefs.HUDDirectory = userPath;
                App.SaveConfiguration();
                App.HudPath = App.Config.ConfigSettings.UserPrefs.HUDDirectory;
                App.Logger.Info($"Target directory set to: {App.HudPath}");
            }
            else
            {
                await ShowMessageBox(Resources.info_path_invalid, MsBox.Avalonia.Enums.Icon.Error);
            }
        }

        // Check one more time if a valid directory has been set.
        return CheckUserPath();
    }

    /// <summary>
    /// Creates the chapterbackgrounds.txt file in the specified path.
    /// </summary>
    public static void CreateChapterBackgroundsFile(string hudPath)
    {
        string content = @"""chapters""
        {
	        1	""background_upward""
	        2	""background_upward""
	        3	""background_upward""
	        4	""background_upward""
        }

        ""BackgroundMaps""
        {
	        1	""background_upward""
	        2	""background_upward""
	        3	""background_upward""
	        4	""background_upward""
        }";

        // Append the path with "/scripts" and create the directory if it doesn't exist.
        hudPath += "/scripts";
        Directory.CreateDirectory(hudPath);

        // Create chapterbackground.txt file with the contents.
        File.WriteAllText(Path.Combine(hudPath, "chapterbackground.txt"), content);
        App.Logger.Info($"Created chapterbackground.txt at {hudPath}");
    }

    /// <summary>
    /// Synchronizes the local HUD schema files with the latest versions on GitHub.
    /// </summary>
    /// <param name="silent">If true, the user will not be notified if there are no updates on startup.</param>
    public static async void UpdateAppSchema(bool silent = true)
    {
        try
        {
            // Create the schema folder if it does not exist.
            if (!Directory.Exists("JSON")) Directory.CreateDirectory("JSON");

            var downloads = new List<Task>();
            var remoteFiles = (await Fetch<GitJson[]>(App.Config.ConfigSettings.AppConfig.JsonListURL)).Where((x) => x.Name.EndsWith(".json") && x.Type == "file").ToArray();

            foreach (var remoteFile in remoteFiles)
            {
                var localFilePath = $"JSON/{remoteFile.Name}";
                bool newFile = false, fileChanged = false;

                if (!File.Exists(localFilePath))
                    newFile = true;
                else
                    fileChanged = remoteFile.SHA != GitHash(localFilePath);

                if (!newFile && !fileChanged) continue;
                App.Logger.Info($"Downloading {remoteFile.Name} ({(newFile ? "newFile" : "")}, {(fileChanged ? "fileChanged" : "")})");
                downloads.Add(DownloadFile(remoteFile.Download, localFilePath));
            }

            // Remove HUD JSONs that aren't available online.
            foreach (var localFile in new DirectoryInfo("JSON").EnumerateFiles())
            {
                if (remoteFiles.Count((x) => x.Name == localFile.Name) == 0)
                {
                    App.Logger.Info($"Deleting {localFile.Name}");
                    File.Delete(localFile.FullName);
                }
            }

            await Task.WhenAll(downloads);
            if (Convert.ToBoolean(downloads.Count))
            {
                if (!silent) if (await ShowPromptBox(Resources.info_hud_update) == ButtonResult.No) return;
                Debug.WriteLine(Assembly.GetExecutingAssembly().Location);
                Process.Start(Assembly.GetExecutingAssembly().Location.Replace(".dll", ".exe"));
                Environment.Exit(0);
            }
            else
            {
                if (!silent) await ShowMessageBox(Resources.info_hud_update_none);
            }
        }
        catch (Exception e)
        {
            App.Logger.Error(e.Message);
            Console.WriteLine(e);
        }
        finally
        {
            UpdateAppVersion();
        }
    }

    /// <summary>
    /// Checks if there's a new version of the app available.
    /// </summary>
    public static async void UpdateAppVersion()
    {
        try
        {
            var localVersion = Assembly.GetExecutingAssembly().GetName().Version;
            var latestVersion = await new GitHubClient(new ProductHeaderValue("TF2HUD.Editor")).Repository.Release.GetLatest("CriticalFlaw", "TF2HUD.Editor");
            App.Logger.Info($"Checking for app update. Latest version is {latestVersion.TagName}");

            // Parse the remote version (remove a leading 'v' if present)
            if (!Version.TryParse(latestVersion.TagName.TrimStart('v'), out var remoteVersion))
            {
                App.Logger.Warn($"Failed to parse remote version: {latestVersion.TagName}");
                return;
            }

            // Only update if remote version is *greater than* the local version
            if (remoteVersion > localVersion)
            {
                App.Logger.Info($"Update available from {localVersion} -> {remoteVersion}");
                if (await ShowPromptBox(Resources.info_app_update) == ButtonResult.No) return;
                OpenWebpage(App.Config.ConfigSettings.AppConfig.LatestUpdateURL);
            }
        }
        catch (Exception e)
        {
            App.Logger.Error(e.Message);
            Console.WriteLine(e);
        }
    }
}