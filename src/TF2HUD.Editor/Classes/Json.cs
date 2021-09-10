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
using log4net;
using Newtonsoft.Json;

namespace HUDEditor.Classes
{
    public class Json : INotifyPropertyChanged
    {
        // Highlighted HUD
        private HUD _highlightedHud;

        // Selected HUD
        private HUD _selectedHud;

        // HUDs to manage
        public HUD[] HUDList;
        private readonly ILog _logger;
        private readonly IUtilities _utilities;
        private readonly INotifier _notifier;
        private readonly ILocalization _localization;
        private readonly IAppSettings _settings;
        private readonly VTF _vtf;
        private readonly IHUDUpdateChecker _hudUpdateChecker;

        public Json(
            ILog logger,
            IUtilities utilities,
            INotifier notifier,
            ILocalization localization,
            IAppSettings settings,
            VTF vtf,
            IHUDUpdateChecker hudTester)
        {
            _logger = logger;
            _utilities = utilities;
            _notifier = notifier;
            _localization = localization;
            _settings = settings;
            _vtf = vtf;
            _hudUpdateChecker = hudTester;

            CreateHUDList();

            var selectedHud = GetHUDByName(_settings.HudSelected);
            HighlightedHUD = selectedHud;
            SelectedHUD = selectedHud;
        }

        private void CreateHUDList()
        {
            var hudList = new List<HUD>();
            var sharedJSON = ReadJSONFile("JSON\\Shared\\shared.json");
            var sharedControlsJSON = ReadJSONFile("JSON\\Shared\\controls.json");

            AddPredefinedHUDs(hudList);
            AddSharedHUDs(hudList, sharedJSON, sharedControlsJSON);
            AddLocalSharedHUDs(hudList, sharedControlsJSON);

            hudList.Sort((a, b) => string.CompareOrdinal(a.Name, b.Name));
            HUDList = hudList.ToArray();
        }

        private static string ReadJSONFile(string path)
        {
            return new StreamReader(File.OpenRead(path), new UTF8Encoding(false)).ReadToEnd();
        }

        private void AddLocalSharedHUDs(List<HUD> hudList, string sharedControlsJSON)
        {
            // Local Shared HUDs
            var localSharedHUDsPath = $@"{Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData)}\\TF2HUD.Editor\\LocalShared";
            var localSharedPath = Directory.CreateDirectory(localSharedHUDsPath).FullName;

            foreach (var localSharedHUD in Directory.EnumerateDirectories(localSharedPath))
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
                }, false, _logger, _utilities, _notifier, _localization, _vtf, _settings));
            }
        }

        private void AddSharedHUDs(List<HUD> hudList, string sharedJSON, string sharedControlsJSON)
        {
            // Load all shared huds from JSON/Shared/shared.json, and shared controls from JSON/Shared/controls.json
            // For each hud, assign unique ids for the controls based on the hud name and add to HUDs list.
            var sharedHUDs = JsonConvert.DeserializeObject<List<HudJson>>(sharedJSON);
            hudList.AddRange(sharedHUDs.Select(hud =>
            {
                var hudControls = JsonConvert.DeserializeObject<HudJson>(sharedControlsJSON);
                foreach (var control in hudControls.Controls.SelectMany(group => hudControls.Controls[group.Key]))
                    control.Name = $"{_utilities.EncodeID(hud.Name)}_{_utilities.EncodeID(control.Name)}";
                hud.Layout = hudControls.Layout;
                hud.Controls = hudControls.Controls;
                return new HUD(hud.Name, hud, false, _logger, _utilities, _notifier, _localization, _vtf, _settings);
            }));
        }

        private void AddPredefinedHUDs(List<HUD> hudList)
        {
            foreach (var jsonFile in Directory.EnumerateFiles("JSON"))
            {
                // Extract HUD information from the file path and add it to the object list.
                var fileInfo = jsonFile.Split("\\")[^1].Split(".");
                if (fileInfo[^1] != "json") continue;
                hudList.Add(new HUD(fileInfo[0], JsonConvert.DeserializeObject<HudJson>(new StreamReader(File.OpenRead(jsonFile), new UTF8Encoding(false)).ReadToEnd()), true, _logger, _utilities, _notifier, _localization, _vtf, _settings));
            }
        }

        public HUD HighlightedHUD
        {
            get => _highlightedHud;
            set
            {
                _highlightedHud = value;
                OnPropertyChanged("HighlightedHUD");
                OnPropertyChanged("HighlightedHUDInstalled");
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
                                            _utilities.CheckUserPath(MainWindow.HudPath) &&
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
            _logger.Info($"OnPropertyChanged: {propertyChanged}");
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyChanged));
        }

        /// <summary>
        ///     Synchronize the local HUD schema files with the latest versions on GitHub.
        /// </summary>
        public bool Update(bool forceDownload = false)
        {
            try
            {
                // Get the local schema names and file sizes.
                List<Tuple<string, int>> localHUDSchemas = GetHUDSchemas();
                if (localHUDSchemas.Count <= 0) return false;

                var client = new WebClient();
                client.Headers.Add("User-Agent", "request");

                List<Tuple<string, int>> remoteFiles = GetRemoteFiles(client);
                if (remoteFiles.Count <= 0) return false;

                var restartRequired = false;
                // Compare the local and remote files.
                foreach (var (remoteName, remoteSize) in remoteFiles)
                {
                    if (!forceDownload && !HudDoesNotExistLocallyOrIsOutdated(localHUDSchemas, remoteName, remoteSize))
                    {
                        _logger.Info($"{remoteName}: No updates...");
                        continue;
                    }

                    DownloadHUD(client, remoteName);

                    restartRequired = true;
                }

                return restartRequired;
            }
            catch (Exception e)
            {
                _logger.Error(e.Message);
                Console.WriteLine(e);
                return false;
            }
        }

        private bool HudDoesNotExistLocallyOrIsOutdated(List<Tuple<string, int>> localHUDSchemas, string remoteName, int remoteSize)
        {
            var shouldDownloadFile = true;
            _logger.Info($"{remoteName}: Checking ...");
            foreach (var (localName, localSize) in localHUDSchemas)
                if (string.Equals(remoteName, localName))
                {
                    // The remote file is found locally. Check if the file size has noticeably changed.
                    shouldDownloadFile = FileSizeHasChanged(remoteSize, localSize);
                    break;
                }

            return shouldDownloadFile;
        }

        private static bool FileSizeHasChanged(int remoteSize, int localSize)
        {
            return !Enumerable.Range(remoteSize - 100, remoteSize + 100).Contains(localSize);
        }

        private void DownloadHUD(WebClient client, string remoteName)
        {
            var fileName = $"{remoteName}.json";
            _logger.Info($"{remoteName}: Downloading latest version...");
            client.DownloadFile(string.Format(_settings.JsonFile, fileName), fileName);
            client.Dispose();

            // Move the fresh file into the JSON folder, overwriting the previous version.
            if (File.Exists(fileName))
                File.Move(fileName, $"JSON/{fileName}", true);
        }

        private List<Tuple<string, int>> GetRemoteFiles(WebClient client)
        {
            var remoteList = DownloadRemoteList(client);

            // Get the remote schema names and file sizes.
            return JsonConvert.DeserializeObject<List<GitJson>>(remoteList)
                .Where(x => x.Name.EndsWith(".json"))
                .Select(file => new Tuple<string, int>(file.Name.Replace(".json", string.Empty), file.Size))
                .ToList();
        }

        private string DownloadRemoteList(WebClient client)
        {
            // Setup the WebClient for download remote files.
            ServicePointManager.Expect100Continue = true;
            ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;
            ServicePointManager.ServerCertificateValidationCallback = delegate { return true; };

            var remoteList = client.DownloadString(_settings.JsonList);
            client.Dispose();
            return remoteList;
        }

        private static List<Tuple<string, int>> GetHUDSchemas()
        {
            var localFiles = new List<Tuple<string, int>>();
            foreach (var file in new DirectoryInfo("JSON").GetFiles().Where(x => x.FullName.EndsWith(".json")))
                localFiles.Add(new Tuple<string, int>(file.Name.Replace(".json", string.Empty), (int)file.Length));
            return localFiles;
        }

        /// <summary>
        /// Checks for HUD schema updates asynchronously.
        /// </summary>
        /// <returns>True if at least one HUD has an update available, false otherwise.</returns>
        public async Task<bool> UpdateAsync()
        {
            try
            {
                return (await Task.WhenAll(HUDList.Select(async hud =>
                {
                    return await IsUpdateAvailable(hud);
                }))).Contains(true);
            }
            catch (Exception e)
            {
                _logger.Error(e.Message);
                Console.WriteLine(e);
                return false;
            }
        }

        /// <summary>
        /// Determines if an update is available for the given HUD.
        /// </summary>
        /// <param name="hud"></param>
        /// <returns>True if an update is available, false otherwise.</returns>
        private async Task<bool> IsUpdateAvailable(HUD hud)
        {
            var url = string.Format(_settings.JsonFile, $"{hud.Name}.json");
            _logger.Info($"Requesting {hud.Name} from {url}");
            var response = await _utilities.Fetch(url);

            if (response is null)
            {
                _logger.Info($"{hud.Name}: Received HTTP error, unable to determine whether HUD has been updated!");
                return false;
            }

            // Test everything except controls and settings. Complex fields require more testing.
            var latestHud = new HUD(hud.Name, JsonConvert.DeserializeObject<HudJson>(response), true, _logger, _utilities, _notifier, _localization, _vtf, _settings);
            return !_hudUpdateChecker.AreEqual(hud.Name, hud, latestHud, new[]
            {
                "controls",
                "DirtyControls",
                "isRendered",
                "Settings"
            });
        }

        public HUD Add(string folderPath)
        {
            var hudName = folderPath.Split('\\')[^1];

            var hudDetailsFolder = $@"{Directory.CreateDirectory($@"{
                Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData)
            }\TF2HUD.Editor\LocalShared\{hudName}").FullName}";

            var hudJson = CreateHudJson(folderPath, hudName, hudDetailsFolder);

            var hud = new HUD(hudName, hudJson, false, _logger, _utilities, _notifier, _localization, _vtf, _settings);
            UpdateHUDList(hud);
            HighlightedHUD = hud;
            SelectedHUD = hud;
            return hud;
        }

        private void UpdateHUDList(HUD hud)
        {
            var hudList = HUDList.ToList();
            hudList.Add(hud);
            hudList.Sort((a, b) => string.Compare(a.Name, b.Name));
            HUDList = hudList.ToArray();
        }

        private HudJson CreateHudJson(string folderPath, string hudName, string hudDetailsFolder)
        {
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
                            _logger.Info($"[Json.Add] Found background file background_{backgrounds[backgroundIndex]}_widescreen.vtf");
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
                                _logger.Info($"[VTF2TGA] {process.StandardOutput.ReadLine()}");
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

            var sharedControlsJSON = ReadJSONFile("JSON\\Shared\\controls.json");
            var hudControls = JsonConvert.DeserializeObject<HudJson>(sharedControlsJSON);
            foreach (var group in hudControls.Controls)
                foreach (var control in hudControls.Controls[group.Key])
                    control.Name = $"{_utilities.EncodeID(hudJson.Name)}_{_utilities.EncodeID(control.Name)}";

            hudJson.Layout = hudControls.Layout;
            hudJson.Controls = hudControls.Controls;

            return hudJson;
        }
    }
}