using HUDEditor.Models;
using HUDEditor.Properties;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace HUDEditor.Classes
{
    public class Json : INotifyPropertyChanged
    {
        // Highlighted HUD
        private HUD _highlightedHud;

        // Selected HUD
        private HUD _selectedHud;

        // HUDs to manage
        public HUD[] HudList;

        public Json()
        {
            var hudList = new List<HUD>();
            foreach (var jsonFile in Directory.EnumerateFiles("JSON"))
            {
                // Extract HUD information from the file path and add it to the object list.
                var fileInfo = jsonFile.Split("\\")[^1].Split(".");
                if (fileInfo[^1] != "json") continue;
                hudList.Add(new HUD(fileInfo[0], JsonConvert.DeserializeObject<HudJson>(new StreamReader(File.OpenRead(jsonFile), new UTF8Encoding(false)).ReadToEnd()), true));
            }

            // Load all shared huds from JSON/Shared/shared.json, and shared controls from JSON/Shared/controls.json
            // For each hud, assign unique ids for the controls based on the hud name and add to HUDs list.
            var sharedHuds = JsonConvert.DeserializeObject<List<HudJson>>(new StreamReader(File.OpenRead("JSON\\Shared\\shared.json"), new UTF8Encoding(false)).ReadToEnd());
            var sharedControlsJson = new StreamReader(File.OpenRead("JSON\\Shared\\controls.json"), new UTF8Encoding(false)).ReadToEnd();
            hudList.AddRange(sharedHuds!.Select(hud =>
            {
                var hudControls = JsonConvert.DeserializeObject<HudJson>(sharedControlsJson);
                foreach (var control in hudControls.Controls.SelectMany(group => hudControls.Controls[group.Key]))
                    control.Name = $"{Utilities.EncodeId(hud.Name)}_{Utilities.EncodeId(control.Name)}";
                hud.Layout = hudControls.Layout;
                hud.Controls = hudControls.Controls;
                return new HUD(hud.Name, hud, false);
            }));

            // Local Shared HUDs
            var localSharedPath = Directory.CreateDirectory($@"{Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData)}\\TF2HUD.Editor\\LocalShared").FullName;

            foreach (var sharedHud in Directory.EnumerateDirectories(localSharedPath))
            {
                var hudName = sharedHud.Split('\\')[^1];
                var hudBackgroundPath = $"{sharedHud}\\output.png";
                var hudBackground = File.Exists(hudBackgroundPath)
                    ? $"file://{hudBackgroundPath}"
                    : "https://user-images.githubusercontent.com/6818236/123523002-0061aa00-d68f-11eb-8c47-a17b47cbaf0c.png";
                var sharedProperties = JsonConvert.DeserializeObject<HudJson>(sharedControlsJson);
                hudList.Add(new HUD(hudName, new HudJson
                {
                    Name = hudName,
                    Thumbnail = hudBackground,
                    Background = hudBackground,
                    Layout = sharedProperties.Layout,
                    Links = new Links
                    {
                        Download = new[] { new Download() { Source = "GitHub", Link = $"file://{sharedHud}\\{hudName}.zip" } }
                    },
                    Controls = sharedProperties.Controls
                }, false));
            }

            hudList.Sort((a, b) => string.CompareOrdinal(a.Name, b.Name));
            HudList = hudList.ToArray();

            var selectedHud = this[Settings.Default.hud_selected];
            HighlightedHud = selectedHud;
            SelectedHud = selectedHud;
        }

        public HUD HighlightedHud
        {
            get => _highlightedHud;
            set
            {
                _highlightedHud = value;
                OnPropertyChanged("HighlightedHUD");
                OnPropertyChanged("HighlightedHUDInstalled");
            }
        }

        public bool HighlightedHudInstalled => HighlightedHud != null && Directory.Exists($"{MainWindow.HudPath}\\{HighlightedHud.Name}");

        public HUD SelectedHud
        {
            get => _selectedHud;
            set
            {
                _selectedHud = value;
                SelectionChanged?.Invoke(this, value);
                OnPropertyChanged("SelectedHUD");
                OnPropertyChanged("SelectedHUDInstalled");
            }
        }

        // Selected HUD Installed
        public bool SelectedHudInstalled => SelectedHud != null && MainWindow.HudPath != "" &&
                                            Directory.Exists(MainWindow.HudPath) &&
                                            Utilities.CheckUserPath(MainWindow.HudPath) &&
                                            MainWindow.CheckHudInstallation();

        public event PropertyChangedEventHandler PropertyChanged;

        public event EventHandler<HUD> SelectionChanged;

        /// <summary>
        ///     Find and retrieve a HUD object selected by the user.
        /// </summary>
        /// <param name="name">Name of the HUD the user wants to view.</param>
        public HUD this[string name]
        {
            get => HudList.FirstOrDefault(hud => string.Equals(hud.Name, name, StringComparison.InvariantCultureIgnoreCase));
        }

        /// <summary>
        ///     Invoke a WPF Binding update of a property.
        /// </summary>
        /// <param name="propertyChanged">Name of property to update</param>
        public void OnPropertyChanged(string propertyChanged)
        {
            MainWindow.Logger.Info($"OnPropertyChanged: {propertyChanged}");
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyChanged));
        }

        /// <summary>
        ///     Synchronize the local HUD schema files with the latest versions on GitHub.
        /// </summary>
        /// <returns>Whether updates are available</returns>
        public async Task<bool> Update()
        {
            try
            {
                var remoteFiles = (await Utilities.Fetch<GitJson[]>(Settings.Default.json_list)).Where((x) => x.Name.EndsWith(".json") && x.Type == "file").ToArray();
                List<Task> downloads = new();

                foreach (var remoteFile in remoteFiles)
                {
                    var localFilePath = $"JSON\\{remoteFile.Name}";
                    bool newFile = false, fileChanged = false;

                    if (!File.Exists(localFilePath))
                        newFile = true;
                    else
                        fileChanged = remoteFile.SHA != Utilities.GitHash(localFilePath);

                    if (!newFile && !fileChanged) continue;
                    MainWindow.Logger.Info($"Downloading {remoteFile.Name} ({(newFile ? "newFile" : "")}, {(fileChanged ? "fileChanged" : "")})");
                    downloads.Add(Utilities.DownloadFile(remoteFile.Download, localFilePath));
                }

                // Remove HUD JSONs that aren't available online.
                foreach (var localFile in new DirectoryInfo("JSON").EnumerateFiles())
                {
                    if (remoteFiles.Count((x) => x.Name == localFile.Name) != 0) continue;
                    MainWindow.Logger.Info($"Deleting {localFile.Name}");
                    File.Delete(localFile.FullName);
                }

                await Task.WhenAll(downloads);
                return Convert.ToBoolean(downloads.Count);
            }
            catch (Exception e)
            {
                MainWindow.Logger.Error(e.Message);
                Console.WriteLine(e);
                return false;
            }
        }

        public async Task<HUD> Add(string folderPath)
        {
            var hudName = folderPath.Split('\\')[^1];

            var hudDetailsFolder = $@"{Directory.CreateDirectory($@"{Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData)}\TF2HUD.Editor\LocalShared\{hudName}").FullName}";

            var hudJson = new HudJson
            {
                Name = hudName,
                Thumbnail = Task.Run(() =>
                {
                    var consoleFolder = $"{folderPath}\\materials\\console";
                    var backgrounds = new[] { "2fort", "gravelpit", "mvm", "upward" };

                    var backgroundSelection = backgrounds.FirstOrDefault(background => File.Exists($"{consoleFolder}\\background_{background}_widescreen.vtf"));
                    if (backgroundSelection is null) return backgroundSelection;

                    var inputPath = $"{consoleFolder}\\background_{backgroundSelection}_widescreen.vtf";

                    MainWindow.Logger.Info($"[Json.Add] Found background file background_{backgroundSelection}_widescreen.vtf");
                    var outputPathTga = $"{consoleFolder}\\output.tga";
                    var outputPath = $"{hudDetailsFolder}\\output";

                    string[] args =
                    {
                        "-i",
                        $"\"{inputPath}\"",
                        "-o",
                        $"\"{outputPathTga}\""
                    };
                    var processInfo = new ProcessStartInfo($"{MainWindow.HudPath.Replace("\\tf\\custom", string.Empty)}\\bin\\vtf2tga.exe")
                    {
                        Arguments = string.Join(" ", args),
                        RedirectStandardOutput = true
                    };
                    var process = Process.Start(processInfo);
                    while (!process.StandardOutput.EndOfStream)
                        MainWindow.Logger.Info($"[VTF2TGA] {process.StandardOutput.ReadLine()}");
                    process.WaitForExit();
                    process.Close();

                    File.Move(outputPathTga, $"{outputPath}.tga", true);

                    var tga = new TGA($"{outputPath}.tga");
                    var rectImage = new Bitmap(
                        (int)SystemParameters.PrimaryScreenWidth,
                        (int)SystemParameters.PrimaryScreenHeight
                    );
                    var graphics = Graphics.FromImage(rectImage);
                    graphics.DrawImage((Image)tga, 0, 0, rectImage.Width, rectImage.Height);
                    rectImage.Save($"{outputPath}.png");

                    return $"file://{outputPath}.png";
                }).Result,
                Links = new Links
                {
                    Download = Task.Run(() =>
                    {
                        var zipPath = $"{hudDetailsFolder}\\{hudName}.zip";
                        ZipFile.CreateFromDirectory(folderPath, zipPath, CompressionLevel.Fastest, true);
                        return new[] { new Download() { Source = "GitHub", Link = $"file://{zipPath}" } };
                    }).Result
                }
            };

            hudJson.Background = hudJson.Thumbnail;

            // TF2 HUD Crosshairs
            await InstallCrosshairs(folderPath);

            var hudList = HudList.ToList();
            var sharedControlsJson = new StreamReader(File.OpenRead("JSON\\Shared\\controls.json"), new UTF8Encoding(false)).ReadToEnd();

            var hudControls = JsonConvert.DeserializeObject<HudJson>(sharedControlsJson);
            foreach (var group in hudControls.Controls)
                foreach (var control in hudControls.Controls[group.Key])
                    control.Name = $"{Utilities.EncodeId(hudJson.Name)}_{Utilities.EncodeId(control.Name)}";

            hudJson.Layout = hudControls.Layout;
            hudJson.Controls = hudControls.Controls;

            var hud = new HUD(hudName, hudJson, false);
            hudList.Add(hud);
            hudList.Sort((a, b) => string.CompareOrdinal(a.Name, b.Name));
            HudList = hudList.ToArray();
            HighlightedHud = hud;
            SelectedHud = hud;
            return hud;
        }

        /// <summary>
        ///     Install the Hypnotize TF2 HUD Crosshairs to a given HUD folder
        /// </summary>
        /// <param name="folderPath">Absolute folder path to HUD to install crosshairs to</param>
        public async Task InstallCrosshairs(string folderPath)
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