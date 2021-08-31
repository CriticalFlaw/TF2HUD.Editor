using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using HUDEditor.Models;
using HUDEditor.Properties;
using Newtonsoft.Json;

namespace HUDEditor.Classes
{
    public class Json : INotifyPropertyChanged
    {
        // Highlighted HUD
        private HUD _highlightedHud;

        // Local Shared HUDs Path
        private string _localSharedPath;

        // Selected HUD
        private HUD _selectedHud;

        // HUDs to manage
        public HUD[] HUDList;

        private readonly MainWindowDelegate _selectedHudSet;
        private readonly MainWindowDelegate _selectedHudCleared;

        public Json(MainWindowDelegate selectedHudSet, MainWindowDelegate selectedHudCleared)
        {
            _selectedHudSet = selectedHudSet;
            _selectedHudCleared = selectedHudCleared;

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
            var sharedHUDs = JsonConvert.DeserializeObject<List<HudJson>>(new StreamReader(File.OpenRead("JSON\\Shared\\shared.json"), new UTF8Encoding(false)).ReadToEnd());
            var sharedControlsJSON = new StreamReader(File.OpenRead("JSON\\Shared\\controls.json"), new UTF8Encoding(false)).ReadToEnd();
            hudList.AddRange(sharedHUDs.Select(hud =>
            {
                var hudControls = JsonConvert.DeserializeObject<HudJson>(sharedControlsJSON);
                foreach (var control in hudControls.Controls.SelectMany(group => hudControls.Controls[group.Key]))
                    control.Name = $"{Utilities.EncodeID(hud.Name)}_{Utilities.EncodeID(control.Name)}";
                hud.Layout = hudControls.Layout;
                hud.Controls = hudControls.Controls;
                return new HUD(hud.Name, hud, false);
            }));

            // Local Shared HUDs
            _localSharedPath = Directory.CreateDirectory($@"{Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData)}\\TF2HUD.Editor\\LocalShared").FullName;

            foreach (var localSharedHUD in Directory.EnumerateDirectories(_localSharedPath))
            {
                var hudName = localSharedHUD.Split('\\')[^1];
                var hudBackgroundPath = $"{localSharedHUD}\\output.png";
                var hudBackground = File.Exists(hudBackgroundPath)
                    ? $"file://{hudBackgroundPath}"
                    : "https://user-images.githubusercontent.com/6818236/123523002-0061aa00-d68f-11eb-8c47-a17b47cbaf0c.png";
                var sharedProperties = JsonConvert.DeserializeObject<HudJson>(sharedControlsJSON);
                hudList.Add(new HUD(hudName, new HudJson
                {
                    Name = hudName,
                    Thumbnail = hudBackground,
                    Background = hudBackground,
                    Layout = sharedProperties.Layout,
                    Links = new Links
                    {
                        Update = $"file://{localSharedHUD}\\{hudName}.zip"
                    },
                    Controls = sharedProperties.Controls
                }, false));
            }

            hudList.Sort((a, b) => string.CompareOrdinal(a.Name, b.Name));
            HUDList = hudList.ToArray();

            var selectedHud = GetHUDByName(Settings.Default.hud_selected);
            HighlightedHUD = selectedHud;
            SelectedHUD = selectedHud;
        }

        public HUD HighlightedHUD
        {
            get => _highlightedHud;
            set
            {
                _highlightedHud = value;
                OnPropertyChanged("HighlightedHUD");
                OnPropertyChanged("HighlightedHUDInstalled");
                if (_highlightedHud != null)
                    _selectedHudSet();
                else
                    _selectedHudCleared();
            }
        }

        public bool HighlightedHUDInstalled => HighlightedHUD != null && Directory.Exists($"{MainWindow.HudPath}\\{HighlightedHUD.Name}");

        public HUD SelectedHUD
        {
            get => _selectedHud;
            set
            {
                _selectedHud = value;
                SelectionChanged?.Invoke(this, value);
                OnPropertyChanged("SelectedHUD");
            }
        }

        // Selected HUD Installed
        public bool SelectedHUDInstalled => SelectedHUD != null && MainWindow.HudPath != "" &&
                                            Directory.Exists(MainWindow.HudPath) &&
                                            Utilities.CheckUserPath(MainWindow.HudPath) &&
                                            MainWindow.CheckHudInstallation();

        public event PropertyChangedEventHandler PropertyChanged;

        public event EventHandler<HUD> SelectionChanged;

        /// <summary>
        ///     Find and retrieve a HUD object selected by the user.
        /// </summary>
        /// <param name="name">Name of the HUD the user wants to view.</param>
        public HUD GetHUDByName(string name)
        {
            return HUDList.FirstOrDefault(hud => string.Equals(hud.Name, name, StringComparison.InvariantCultureIgnoreCase));
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
        public static bool Update(bool force = false)
        {
            try
            {
                // Get the local schema names and file sizes.
                var localFiles = new List<Tuple<string, int>>();
                foreach (var file in new DirectoryInfo("JSON").GetFiles().Where(x => x.FullName.EndsWith(".json")))
                    localFiles.Add(new Tuple<string, int>(file.Name.Replace(".json", string.Empty), (int)file.Length));
                if (localFiles.Count <= 0) return false;

                // Setup the WebClient for download remote files.
                ServicePointManager.Expect100Continue = true;
                ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;
                ServicePointManager.ServerCertificateValidationCallback = delegate { return true; };
                var client = new WebClient();
                client.Headers.Add("User-Agent", "request");
                var remoteList = client.DownloadString(Settings.Default.json_list);
                client.Dispose();

                // Get the remote schema names and file sizes.
                var remoteFiles = JsonConvert.DeserializeObject<List<GitJson>>(remoteList)
                    .Where(x => x.Name.EndsWith(".json"))
                    .Select(file => new Tuple<string, int>(file.Name.Replace(".json", string.Empty), file.Size))
                    .ToList();
                if (remoteFiles.Count <= 0) return false;

                var restartRequired = false;
                // Compare the local and remote files.
                foreach (var (remoteName, remoteSize) in remoteFiles)
                {
                    if (!force)
                    {
                        var downloadFile = true;
                        MainWindow.Logger.Info($"{remoteName}: Checking ...");
                        foreach (var (localName, localSize) in localFiles)
                            if (string.Equals(remoteName, localName))
                            {
                                // The remote file is found locally. Check if the file size has noticeably changed.
                                downloadFile = !Enumerable.Range(remoteSize - 100, remoteSize + 100).Contains(localSize);
                                break;
                            }

                        // If the remote file is not found locally, or the size difference is too great - download the latest version.
                        if (!downloadFile)
                        {
                            MainWindow.Logger.Info($"{remoteName}: No updates...");
                            continue;
                        }
                    }

                    var fileName = $"{remoteName}.json";
                    MainWindow.Logger.Info($"{remoteName}: Downloading latest version...");
                    client.DownloadFile(string.Format(Settings.Default.json_file, fileName), fileName);
                    client.Dispose();

                    // Move the fresh file into the JSON folder, overwriting the previous version.
                    if (File.Exists(fileName))
                        File.Move(fileName, $"JSON/{fileName}", true);

                    restartRequired = true;
                }

                return restartRequired;
            }
            catch (Exception e)
            {
                MainWindow.Logger.Error(e.Message);
                Console.WriteLine(e);
                return false;
            }
        }

        public async Task<bool> UpdateAsync()
        {
            try
            {
                return (await Task.WhenAll(HUDList.Select(async x =>
                {
                    var url = string.Format(Settings.Default.json_file, $"{x.Name}.json");
                    MainWindow.Logger.Info($"Requesting {x.Name} from {url}");
                    var response = await Utilities.Fetch(url);
                    if (response != null)
                        return !x.TestHUD(new HUD(x.Name, JsonConvert.DeserializeObject<HudJson>(response), true));
                    MainWindow.Logger.Info($"{x.Name}: Received HTTP error, unable to determine whether HUD has been updated!");
                    return false;
                }))).Contains(true);
            }
            catch (Exception e)
            {
                MainWindow.Logger.Error(e.Message);
                Console.WriteLine(e);
                return false;
            }
        }

        public HUD Add(string folderPath)
        {
            var hudName = folderPath.Split('\\')[^1];

            var hudDetailsFolder = $@"{Directory.CreateDirectory($@"{
                Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData)
            }\TF2HUD.Editor\LocalShared\{hudName}").FullName}";

            var hudJson = new HudJson
            {
                Name = hudName,
                Thumbnail = Task.Run(() =>
                {
                    var consoleFolder = $"{folderPath}\\materials\\console";
                    var backgrounds = new[] { "2fort", "gravelpit", "mvm", "upward" };
                    var backgroundIndex = 0;
                    string backgroundUri = null;

                    while (backgroundUri == null && backgroundIndex < backgrounds.Length)
                    {
                        var inputPath = $"{consoleFolder}\\background_{backgrounds[backgroundIndex]}_widescreen.vtf";

                        if (File.Exists(inputPath))
                        {
                            MainWindow.Logger.Info($"[Json.Add] Found background file background_{backgrounds[backgroundIndex]}_widescreen.vtf");
                            var vtf2tgaOutputPath = $"{consoleFolder}\\output.tga";
                            var outputPath = $"{hudDetailsFolder}\\output";

                            string[] args =
                            {
                                "-i",
                                $"\"{inputPath}\"",
                                "-o",
                                $"\"{vtf2tgaOutputPath}\""
                            };
                            var processInfo = new ProcessStartInfo($"{MainWindow.HudPath.Replace("\\tf\\custom", string.Empty)}\\bin\\vtf2tga.exe")
                            {
                                Arguments = string.Join(" ", args),
                                RedirectStandardOutput = true
                            };
                            var process = Process.Start(processInfo);
                            while (!process.StandardOutput.EndOfStream)
                                MainWindow.Logger.Info($"[VTF2TGA] {process.StandardOutput.ReadLine()}");
                            process?.WaitForExit();
                            process?.Close();

                            File.Move(vtf2tgaOutputPath, $"{outputPath}.tga", true);

                            var tga = new TGA($"{outputPath}.tga");
                            var rectImage = new Bitmap(
                                (int)SystemParameters.PrimaryScreenWidth,
                                (int)SystemParameters.PrimaryScreenHeight
                            );
                            var graphics = Graphics.FromImage(rectImage);
                            graphics.DrawImage((Image)tga, 0, 0, rectImage.Width, rectImage.Height);
                            rectImage.Save($"{outputPath}.png");

                            backgroundUri = $"file://{outputPath}.png";
                        }

                        backgroundIndex++;
                    }

                    return backgroundUri;
                }).Result,
                Links = new Links
                {
                    Update = Task.Run(() =>
                    {
                        var zipPath = $"{hudDetailsFolder}\\{hudName}.zip";
                        ZipFile.CreateFromDirectory(folderPath, zipPath, CompressionLevel.Fastest, true);
                        return $"file://{zipPath}";
                    }).Result
                }
            };

            hudJson.Background = hudJson.Thumbnail;

            var hudList = HUDList.ToList();
            var sharedControlsJSON = new StreamReader(File.OpenRead("JSON\\Shared\\controls.json"), new UTF8Encoding(false)).ReadToEnd();

            var hudControls = JsonConvert.DeserializeObject<HudJson>(sharedControlsJSON);
            foreach (var group in hudControls.Controls)
            foreach (var control in hudControls.Controls[group.Key])
                control.Name = $"{Utilities.EncodeID(hudJson.Name)}_{Utilities.EncodeID(control.Name)}";

            hudJson.Layout = hudControls.Layout;
            hudJson.Controls = hudControls.Controls;

            var hud = new HUD(hudName, hudJson, false);
            hudList.Add(hud);
            hudList.Sort((a, b) => string.Compare(a.Name, b.Name));
            HUDList = hudList.ToArray();
            HighlightedHUD = hud;
            SelectedHUD = hud;
            return hud;
        }
    }
}