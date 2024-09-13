using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Forms;
using CommunityToolkit.Mvvm.Input;
using Crews.Utility.TgaSharp;
using HUDEditor.Classes;
using HUDEditor.Models;
using HUDEditor.Properties;
using Microsoft.Win32;
using Newtonsoft.Json;

namespace HUDEditor.ViewModels
{
    public partial class MainWindowViewModel : ViewModelBase
    {
        private List<HUD> _hudList;
        public IEnumerable<HUD> HUDList => _hudList;
        private HUD _highlightedHud;
        public HUD HighlightedHud
        {
            get => _highlightedHud;
            set
            {
                _highlightedHud = value;
                OnPropertyChanged(nameof(HighlightedHud));
                OnPropertyChanged(nameof(HighlightedHudInstalled));
            }
        }

        public bool HighlightedHudInstalled => MainWindow.CheckHudInstallation(HighlightedHud);
        private HUD _selectedHud;
        public HUD SelectedHud
        {
            get => _selectedHud;
            private set
            {
                _highlightedHud = null;
                _selectedHud = value;
                OnPropertyChanged(nameof(SelectedHud));
                OnPropertyChanged(nameof(SelectedHudInstalled));

                Page?.Dispose();
                Page = _selectedHud != null ? new EditHUDViewModel(this, SelectedHud) : new HomePageViewModel(this, HUDList);
                MainWindow.Logger.Info($"Changing page view to: {(_selectedHud?.Name ?? "Home")}");

                Settings.Default.hud_selected = SelectedHud?.Name ?? string.Empty;
                Settings.Default.Save();
            }
        }

        public bool SelectedHudInstalled => MainWindow.CheckHudInstallation(SelectedHud);
        private ViewModelBase _page;
        public ViewModelBase Page
        {
            get => _page;
            private set
            {
                _page = value;
                OnPropertyChanged(nameof(Page));
            }
        }

        private bool _installing;
        public bool Installing
        {
            get => _installing;
            set
            {
                _installing = value;
                OnPropertyChanged(nameof(Installing));
                BtnInstall_ClickCommand.NotifyCanExecuteChanged();
                BtnUninstall_ClickCommand.NotifyCanExecuteChanged();
            }
        }

        public MainWindowViewModel()
        {
            try
            {
                _hudList = new List<HUD>();
                var sharedControlsJson = new StreamReader(File.OpenRead("JSON\\shared-hud.json"), new UTF8Encoding(false)).ReadToEnd();

                foreach (var jsonFile in Directory.EnumerateFiles("JSON"))
                {
                    // Extract HUD information from the file path and add it to the object list.
                    var fileInfo = jsonFile.Split("\\")[^1].Split(".");
                    if (fileInfo[^1] != "json" || fileInfo[0] == "shared-hud") continue;

                    if (fileInfo[0].Equals("common"))
                    {
                        // Load all common HUDS from `JSON/common.json` and shared controls from `JSON/shared-hud.json`
                        // For each hud, assign unique ids for the controls based on the hud name and add to HUDs list.
                        var sharedHuds = JsonConvert.DeserializeObject<List<HudJson>>(new StreamReader(File.OpenRead("JSON\\common.json"), new UTF8Encoding(false)).ReadToEnd());

                        foreach (var sharedHud in sharedHuds)
                        {
                            var hudControls = JsonConvert.DeserializeObject<HudJson>(sharedControlsJson);
                            foreach (var control in hudControls.Controls.SelectMany(group => hudControls.Controls[group.Key]))
                                control.Name = $"{Utilities.EncodeId(sharedHud.Name)}_{Utilities.EncodeId(control.Name)}";
                            sharedHud.Layout = hudControls.Layout;
                            sharedHud.Controls = hudControls.Controls;
                            _hudList.Add(new HUD(sharedHud.Name, sharedHud, false));
                        }
                    }
                    else
                    {
                        _hudList.Add(new HUD(fileInfo[0], JsonConvert.DeserializeObject<HudJson>(new StreamReader(File.OpenRead(jsonFile), new UTF8Encoding(false)).ReadToEnd()), true));
                    }
                }

                // Local Shared HUDs
                var localSharedPath = Directory.CreateDirectory($@"{Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData)}\\TF2HUD.Editor\\LocalShared").FullName;

                foreach (var sharedHud in Directory.EnumerateDirectories(localSharedPath))
                {
                    var hudName = sharedHud.Split('\\')[^1];
                    var hudBackgroundPath = $"{sharedHud}\\output.png";
                    var hudBackground = File.Exists(hudBackgroundPath)
                        ? $"file://{hudBackgroundPath}"
                        : Settings.Default.app_default_bg;
                    var sharedProperties = JsonConvert.DeserializeObject<HudJson>(sharedControlsJson);
                    _hudList.Add(new HUD(hudName, new HudJson
                    {
                        Name = hudName,
                        Thumbnail = hudBackground,
                        Background = hudBackground,
                        Layout = sharedProperties.Layout,
                        Links = new Links
                        {
                            Download = [new Download() { Source = "GitHub", Link = $"file://{sharedHud}\\{hudName}.zip" }]
                        },
                        Controls = sharedProperties.Controls
                    }, false));
                }

                var selectedHud = this[Settings.Default.hud_selected];
                _highlightedHud = selectedHud;
                _selectedHud = selectedHud;
                _page = selectedHud != null ? new EditHUDViewModel(this, selectedHud) : new HomePageViewModel(this, HUDList);
            }
            catch (Exception e)
            {
                MainWindow.Logger.Error(e.Message);
                Console.WriteLine(e);
            }
        }

        /// <summary>
        /// Retrieves the HUD object selected by user.
        /// </summary>
        /// <param name="name">Name of the HUD the user wants to view.</param>
        public HUD this[string name] => HUDList.FirstOrDefault(hud => string.Equals(hud.Name, name, StringComparison.InvariantCultureIgnoreCase));

        [RelayCommand]
        public void HighlightHUD(HUD hud)
        {
            if (HighlightedHud == hud)
                SelectedHud = hud;
            else
                HighlightedHud = hud;
        }

        [RelayCommand]
        public void SelectHUD(HUD hud)
        {
            SelectedHud = hud;
        }

        #region CLICK_EVENTS

        public bool CanInstall()
        {
            return !Installing;
        }

        /// <summary>
        /// Invokes HUD installation or setting the tf/custom directory, if not already set.
        /// </summary>
        [RelayCommand(CanExecute = nameof(CanInstall))]
        public async void BtnInstall_Click()
        {
            try
            {
                Installing = true;
                SelectedHud ??= HighlightedHud;

                // Force the user to set a directory before installing.
                if (!Utilities.CheckUserPath(MainWindow.HudPath))
                    MainWindow.SetupDirectory(true);

                // Stop the process if Team Fortress 2 is still running.
                if (Utilities.CheckIsGameRunning())
                {
                    Installing = false;
                    return;
                }

                // Clear tf/custom directory of other installed HUDs.
                foreach (var x in HUDList)
                {
                    if (Directory.Exists($"{MainWindow.HudPath}\\{x.Name.ToLowerInvariant()}"))
                    {
                        MainWindow.Logger.Info($"Removing {x.Name.ToLowerInvariant()} from {MainWindow.HudPath}");
                        Directory.Delete($"{MainWindow.HudPath}\\{x.Name.ToLowerInvariant()}", true);
                    }
                }

                // Check for unsupported HUDs in the tf/custom folder. Notify user if found.
                var download = ((EditHUDViewModel)Page).SelectedDownloadSource;
                foreach (var foundHud in Directory.GetDirectories(MainWindow.HudPath))
                {
                    if (!foundHud.Remove(0, MainWindow.HudPath.Length).ToLowerInvariant().Contains("hud") || !File.Exists($"{foundHud}\\info.vdf")) continue;
                    if (MainWindow.ShowMessageBox(MessageBoxImage.Warning, Resources.info_unsupported_hud_found, MessageBoxButton.YesNoCancel) != MessageBoxResult.Yes)
                    {
                        Installing = false;
                        return;
                    }
                    Directory.Delete(foundHud, true);
                }

                // Retrieve the HUD object, then download and extract it into the tf/custom directory.
                MainWindow.Logger.Info($"Downloading {SelectedHud.Name} from {download.Link}");
                HttpClient client = new();
                client.DefaultRequestHeaders.Add("User-Agent", "request");

                var uri = new Uri(download.Link);
                var bytes = uri.Scheme == "file"
                    ? await File.ReadAllBytesAsync(uri.AbsolutePath)
                    : await client.GetByteArrayAsync(uri);

                if (bytes.Length == 0)
                {
                    // GameBanana returns 200 with an empty response for missing download links.
                    throw new HttpRequestException($"Response from {download.Source} did not return a valid zip file");
                }

                // Create new ZIP object from bytes.
                var stream = new MemoryStream(bytes);
                var archive = new ZipArchive(stream);

                // Zip files made with ZipFile.CreateFromDirectory do not include directory entries, so create root directory*
                Directory.CreateDirectory($"{MainWindow.HudPath}\\{SelectedHud.Name}");

                foreach (var entry in archive.Entries)
                {
                    // Remove first folder name from entry.FullName e.g. "flawhud-master" => "".
                    var path = String.Join('\\', entry.FullName.Split("/")[1..]);

                    // Ignore directory entries
                    // path == "" is root directory entry
                    if (path != "" && !path.EndsWith('\\'))
                    {
                        // *and ensure directory exists for each file
                        Directory.CreateDirectory($"{MainWindow.HudPath}\\{SelectedHud.Name}\\{Path.GetDirectoryName(path)}");
                        entry.ExtractToFile($"{MainWindow.HudPath}\\{SelectedHud.Name}\\{path}");
                    }
                }

                // Install Crosshairs
                if (SelectedHud.InstallCrosshairs)
                {
                    MainWindow.Logger.Info($"Installing crosshairs to {SelectedHud.Name}");
                    await Utilities.InstallCrosshairs($"{MainWindow.HudPath}\\{SelectedHud.Name}");
                }

                // Update the page view.
                if (string.IsNullOrWhiteSpace(SelectedHud.Name))
                {
                    Installing = false;
                    return;
                }
                SelectedHud.Settings.SaveSettings();
                SelectedHud.ApplyCustomizations();

                // Update timestamp
                ((EditHUDViewModel)Page).Status = string.Format(Resources.status_installed_now, Settings.Default.hud_selected, DateTime.Now);

                // Update Install/Uninstall/Reset Buttons
                OnPropertyChanged(nameof(HighlightedHudInstalled));

                // Update Switch HUD Button
                OnPropertyChanged(nameof(SelectedHud));
                OnPropertyChanged(nameof(SelectedHudInstalled));
                Installing = false;

                // Clean the application directory.
                archive.Dispose();
            }
            catch (Exception e)
            {
                MainWindow.ShowMessageBox(MessageBoxImage.Error, $"{string.Format(Utilities.GetLocalizedString("error_hud_install"), SelectedHud.Name)} {e.Message}");
                Installing = false;
            }
        }

        /// <summary>
        /// Invokes HUD deletion from the tf/custom directory.
        /// </summary>
        [RelayCommand]
        public void BtnUninstall_Click()
        {
            try
            {
                // Check if the HUD is installed in a valid directory.
                if (!SelectedHudInstalled) return;

                // Stop the process if Team Fortress 2 is still running.
                if (Utilities.CheckIsGameRunning()) return;

                // Remove the HUD from the tf/custom directory.
                MainWindow.Logger.Info($"Removing {SelectedHud.Name} from {MainWindow.HudPath}");
                if (SelectedHud.Name != "") Directory.Delete($"{MainWindow.HudPath}\\{SelectedHud.Name}", true);

                OnPropertyChanged(nameof(HighlightedHud));
                OnPropertyChanged(nameof(HighlightedHudInstalled));
                OnPropertyChanged(nameof(SelectedHud));
                OnPropertyChanged(nameof(SelectedHudInstalled));
            }
            catch (Exception e)
            {
                MainWindow.ShowMessageBox(MessageBoxImage.Error, $"{string.Format(Utilities.GetLocalizedString("error_hud_uninstall"), SelectedHud.Name)} {e.Message}");
            }
        }

        /// <summary>
        /// Saves and applies user settings to the HUD files.
        /// </summary>
        [RelayCommand]
        public void BtnSave_Click()
        {
            if (SelectedHud == null) return;

            var selection = SelectedHud;
            if ((Process.GetProcessesByName("hl2").Any() || Process.GetProcessesByName("tf").Any() || Process.GetProcessesByName("tf_win64").Any()) && selection.DirtyControls.Count > 0)
            {
                var message = selection.DirtyControls.Aggregate(Resources.info_game_restart, (current, control) => current + $"\n - {control}");
                if (MainWindow.ShowMessageBox(MessageBoxImage.Question, message) != MessageBoxResult.OK) return;
            }

            MainWindow.Logger.Info("------");
            MainWindow.Logger.Info("Applying user settings");
            selection.Settings.SaveSettings();
            selection.ApplyCustomizations();
            selection.DirtyControls.Clear();

            ((EditHUDViewModel)Page).Status = string.Format(Resources.status_applied, selection.Name, DateTime.Now);
        }

        /// <summary>
        /// Resets user settings for the selected HUD to their default values.
        /// </summary>
        [RelayCommand]
        public void BtnReset_Click()
        {
            // Ask the user if they want to reset before doing so.
            if (MainWindow.ShowMessageBox(MessageBoxImage.Question, Resources.info_hud_reset, MessageBoxButton.YesNo) != MessageBoxResult.Yes) return;

            MainWindow.Logger.Info("------");
            MainWindow.Logger.Info("Resetting user settings");
            var selection = SelectedHud;
            selection.ResetAll();
            selection.Settings.SaveSettings();
            selection.ApplyCustomizations();
            selection.DirtyControls.Clear();
            ((EditHUDViewModel)Page).Status = string.Format(Resources.status_reset, selection.Name, DateTime.Now);
        }

        /// <summary>
        /// Returns to the HUD selection page.
        /// </summary>
        [RelayCommand]
        public void BtnSwitch_Click()
        {
            MainWindow.Logger.Info("Changing page view to: main menu");
            HighlightedHud = null;
            SelectedHud = null;
        }

        /// <summary>
        /// Opens the settings menu for the editor.
        /// </summary>
        [RelayCommand]
        public void BtnSettings_Click()
        {
            var settings = new SettingsWindow();
            settings.Owner = System.Windows.Application.Current.MainWindow;
            settings.Show();
        }

        /// <summary>
        /// Opens the issue tracker for the editor.
        /// </summary>
        [RelayCommand]
        private void BtnReportIssue_Click()
        {
            Utilities.OpenWebpage(Settings.Default.app_tracker);
        }

        /// <summary>
        /// Opens the project documentation site.
        /// </summary>
        [RelayCommand]
        private void BtnDocumentation_Click()
        {
            Utilities.OpenWebpage(Settings.Default.app_docs);
        }

        /// <summary>
        /// Opens the settings menu for the editor.
        /// </summary>
        [RelayCommand]
        public void BtnPlayTF2_Click()
        {
            var is64Bit = Environment.Is64BitProcess ? "Wow6432Node\\" : string.Empty;
            var steamPath = (string)Registry.GetValue($@"HKEY_LOCAL_MACHINE\Software\{is64Bit}Valve\Steam", "InstallPath", null) + "\\steam.exe";
            Process.Start(steamPath, "steam://rungameid/440");
        }

        /// <summary>
        /// Adds a HUD from folder to the shared HUDs list.
        /// </summary>
        [RelayCommand]
        private async void BtnAddSharedHUD_Click()
        {
            try
            {
                if (MainWindow.ShowMessageBox(MessageBoxImage.Information, Resources.info_add_hud, MessageBoxButton.YesNo) != MessageBoxResult.Yes) return;

                var browser = new FolderBrowserDialog
                {
                    SelectedPath = $@"{MainWindow.HudPath}\"
                };
                if (browser.ShowDialog() != DialogResult.OK) return;

                await Add(browser.SelectedPath);
            }
            catch (Exception error)
            {
                MainWindow.ShowMessageBox(MessageBoxImage.Error, error.Message);
            }
        }

        #endregion CLICK_EVENTS

        /// <summary>
        /// Synchronizes the local HUD schema files with the latest versions on GitHub.
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
                    File.Delete(localFile.FullName);
                    MainWindow.Logger.Info($"Removed {localFile.Name}");
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

        public async Task Add(string folderPath)
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

                    MainWindow.Logger.Info($"Found background file background_{backgroundSelection}_widescreen.vtf");
                    var inputPath = $"{consoleFolder}\\background_{backgroundSelection}_widescreen.vtf";
                    var outputPathTga = $"{consoleFolder}\\output.tga";
                    var outputPath = $"{hudDetailsFolder}\\output";

                    string[] args =
                    [
                        "-i",
                        $"\"{inputPath}\"",
                        "-o",
                        $"\"{outputPathTga}\""
                    ];
                    var processInfo = new ProcessStartInfo($"{MainWindow.HudPath.Replace("\\tf\\custom", string.Empty)}\\bin\\vtf2tga.exe")
                    {
                        Arguments = string.Join(" ", args),
                        RedirectStandardOutput = true
                    };
                    var process = Process.Start(processInfo);
                    while (!process.StandardOutput.EndOfStream)
                        MainWindow.Logger.Info(process.StandardOutput.ReadLine());
                    process.WaitForExit();
                    process.Close();

                    File.Move(outputPathTga, $"{outputPath}.tga", true);

                    var tga = new TGA($"{outputPath}.tga");
                    var rectImage = new Bitmap((int)SystemParameters.PrimaryScreenWidth, (int)SystemParameters.PrimaryScreenHeight);
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
            await Utilities.InstallCrosshairs(folderPath);

            var sharedControlsJson = new StreamReader(File.OpenRead("JSON\\shared-hud.json"), new UTF8Encoding(false)).ReadToEnd();
            var hudControls = JsonConvert.DeserializeObject<HudJson>(sharedControlsJson);
            foreach (var group in hudControls.Controls)
            {
                foreach (var control in hudControls.Controls[group.Key])
                    control.Name = $"{Utilities.EncodeId(hudJson.Name)}_{Utilities.EncodeId(control.Name)}";
            }

            hudJson.Layout = hudControls.Layout;
            hudJson.Controls = hudControls.Controls;

            var hud = new HUD(hudName, hudJson, false);
            _hudList.Add(hud);
            _hudList.Sort((a, b) => string.CompareOrdinal(a.Name, b.Name));

            SelectedHud = hud;
        }
    }
}