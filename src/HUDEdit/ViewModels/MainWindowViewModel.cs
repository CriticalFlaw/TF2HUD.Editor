using CommunityToolkit.Mvvm.Input;
using Crews.Utility.TgaSharp;
using HUDEdit.Classes;
using HUDEdit.Views;
using Microsoft.Win32;
using Newtonsoft.Json;
using HUDEdit.Models;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using Avalonia.Platform;

namespace HUDEdit.ViewModels;

internal partial class MainWindowViewModel : ViewModelBase
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
    
    public bool HighlightedHudInstalled => Utilities.CheckHudInstallation(HighlightedHud);
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

            CurrentPageViewModel?.Dispose();
            CurrentPageViewModel = _selectedHud != null ? new EditHUDViewModel(this, SelectedHud) : new HomePageViewModel(this, HUDList);
            App.Logger.Info($"Changing page view to: {(_selectedHud?.Name ?? "Home")}");

            App.Config.ConfigSettings.UserPrefs.SelectedHUD = SelectedHud?.Name ?? string.Empty;
            App.SaveConfiguration();
        }
    }

    public bool SelectedHudInstalled => Utilities.CheckHudInstallation(SelectedHud);
    private ViewModelBase _currentPageViewModel;
    public ViewModelBase CurrentPageViewModel
    {
        get => _currentPageViewModel;
        private set
        {
        	_currentPageViewModel = value;
        	OnPropertyChanged(nameof(CurrentPageViewModel));
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

    private string _windowTitle = Assets.Resources.ui_title;
    public string WindowTitle
    {
        get => _windowTitle;
        set
        {
            _windowTitle = value;
            OnPropertyChanged();
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
            var localSharedPath = Directory.CreateDirectory($@"JSON\\Local").FullName;

            foreach (var sharedHud in Directory.EnumerateDirectories(localSharedPath))
            {
                var hudName = sharedHud.Split('\\')[^1];
                var hudBackgroundPath = $"{sharedHud}\\output.png";
                var hudBackground = File.Exists(hudBackgroundPath)
                    ? $"file://{hudBackgroundPath}"
                    : "avares://HUDEdit/Assets/Images/background.png";
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

            var selectedHud = this[App.Config.ConfigSettings.UserPrefs.SelectedHUD];
            _highlightedHud = selectedHud;
            _selectedHud = selectedHud;
            _currentPageViewModel = selectedHud != null ? new EditHUDViewModel(this, selectedHud) : new HomePageViewModel(this, HUDList);

            // Load thumbnails
            foreach (var hud in _hudList)
                hud.ThumbnailImage = Utilities.LoadImage(hud.Thumbnail);
        }
        catch (Exception e)
        {
            App.Logger.Error(e.Message);
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
                MainWindow.SetupDirectoryAsync(true);

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
                    App.Logger.Info($"Removing {x.Name.ToLowerInvariant()} from {MainWindow.HudPath}");
                    Directory.Delete($"{MainWindow.HudPath}\\{x.Name.ToLowerInvariant()}", true);
                }
            }

            // Check for unsupported HUDs in the tf/custom folder. Notify user if found.
            var download = ((EditHUDViewModel)CurrentPageViewModel).SelectedDownloadSource;
            foreach (var foundHud in Directory.GetDirectories(MainWindow.HudPath))
            {
                if (!foundHud.Remove(0, MainWindow.HudPath.Length).ToLowerInvariant().Contains("hud") || !File.Exists($"{foundHud}\\info.vdf")) continue;
                if (Utilities.ShowMessageBox(MessageBoxImage.Warning, Assets.Resources.info_unsupported_hud_found, MessageBoxButton.YesNoCancel) != MessageBoxResult.Yes)
                {
                    Installing = false;
                    return;
                }
                Directory.Delete(foundHud, true);
            }

            // Download and install the selected HUD
            await Utilities.DownloadHud(download.Link, MainWindow.HudPath, SelectedHud.Name);

            // Install Crosshairs
            if (SelectedHud.InstallCrosshairs)
            {
                App.Logger.Info($"Installing crosshairs to {SelectedHud.Name}");
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
            ((EditHUDViewModel)CurrentPageViewModel).Status = string.Format(Assets.Resources.status_installed_now, App.Config.ConfigSettings.UserPrefs.SelectedHUD, DateTime.Now);

            // Update Menu Buttons
            OnPropertyChanged(nameof(HighlightedHudInstalled));
            OnPropertyChanged(nameof(SelectedHud));
            OnPropertyChanged(nameof(SelectedHudInstalled));
            Installing = false;
        }
        catch (Exception e)
        {
            Utilities.ShowMessageBox(MessageBoxImage.Error, $"{string.Format(Utilities.GetLocalizedString("error_hud_install"), SelectedHud.Name)} {e.Message}");
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
            App.Logger.Info($"Removing {SelectedHud.Name} from {MainWindow.HudPath}");
            if (SelectedHud.Name != "") Directory.Delete($"{MainWindow.HudPath}\\{SelectedHud.Name}", true);

            // Update timestamp
            ((EditHUDViewModel)CurrentPageViewModel).Status = string.Format(Assets.Resources.status_installed_not, App.Config.ConfigSettings.UserPrefs.SelectedHUD, DateTime.Now);

            // Update Menu Buttons
            OnPropertyChanged(nameof(HighlightedHud));
            OnPropertyChanged(nameof(HighlightedHudInstalled));
            OnPropertyChanged(nameof(SelectedHud));
            OnPropertyChanged(nameof(SelectedHudInstalled));
        }
        catch (Exception e)
        {
            Utilities.ShowMessageBox(MessageBoxImage.Error, $"{string.Format(Utilities.GetLocalizedString("error_hud_uninstall"), SelectedHud.Name)} {e.Message}");
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
            var message = selection.DirtyControls.Aggregate(Assets.Resources.info_game_restart, (current, control) => current + $"\n - {control}");
            if (Utilities.ShowMessageBox(MessageBoxImage.Question, message) != MessageBoxResult.OK) return;
        }

        App.Logger.Info("------");
        App.Logger.Info("Applying user settings");
        selection.Settings.SaveSettings();
        selection.ApplyCustomizations();
        selection.DirtyControls.Clear();

        ((EditHUDViewModel)CurrentPageViewModel).Status = string.Format(Assets.Resources.status_applied, selection.Name, DateTime.Now);
    }

    /// <summary>
    /// Resets user settings for the selected HUD to their default values.
    /// </summary>
    [RelayCommand]
    public void BtnReset_Click()
    {
        // Ask the user if they want to reset before doing so.
        if (Utilities.ShowMessageBox(MessageBoxImage.Question, Assets.Resources.info_hud_reset, MessageBoxButton.YesNo) != MessageBoxResult.Yes) return;

        App.Logger.Info("------");
        App.Logger.Info("Resetting user settings");
        var selection = SelectedHud;
        selection.ResetAll();
        selection.Settings.SaveSettings();
        selection.ApplyCustomizations();
        selection.DirtyControls.Clear();
        ((EditHUDViewModel)CurrentPageViewModel).Status = string.Format(Assets.Resources.status_reset, selection.Name, DateTime.Now);
    }

    /// <summary>
    /// Returns to the HUD selection page.
    /// </summary>
    [RelayCommand]
    public void BtnSwitch_Click()
    {
        App.Logger.Info("Changing page view to: main menu");
        HighlightedHud = null;
        SelectedHud = null;
        WindowTitle = Assets.Resources.ui_title;
    }

    public void OpenDocSite() => Utilities.OpenWebpage(App.Config.ConfigSettings.AppConfig.DocumentationURL);

    public void OpenIssueTracker() => Utilities.OpenWebpage(App.Config.ConfigSettings.AppConfig.IssueTrackerURL);

    public void OpenOptionsMenu()
    {
        var settings = new SettingsView();
        settings.Show();
    }

    /// <summary>
    /// Opens the settings menu for the editor.
    /// </summary>
    [RelayCommand]
    public void LaunchTf2()
    {
        var is64Bit = Environment.Is64BitProcess ? "Wow6432Node\\" : string.Empty;
        var steamPath = (string)Registry.GetValue($@"HKEY_LOCAL_MACHINE\Software\{is64Bit}Valve\Steam", "InstallPath", null) + "\\steam.exe";
        Process.Start(steamPath, "steam://rungameid/440");
    }

    /// <summary>
    /// Adds a HUD from folder to the shared HUDs list.
    /// </summary>
    [RelayCommand]
    public async void AddSharedHud()
    {
        try
        {
            if (Utilities.ShowMessageBox(MessageBoxImage.Information, Assets.Resources.info_add_hud, MessageBoxButton.YesNo) != MessageBoxResult.Yes) return;

            var browser = new OpenFolderDialog
            {
                InitialDirectory = $@"{MainWindow.HudPath}\"
            };
            if (browser.ShowDialog() != true) return;

            await Add(browser.FolderName);
        }
        catch (Exception error)
        {
            Utilities.ShowMessageBox(MessageBoxImage.Error, error.Message);
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
            var remoteFiles = (await Utilities.Fetch<GitJson[]>(App.Config.ConfigSettings.AppConfig.JsonListURL)).Where((x) => x.Name.EndsWith(".json") && x.Type == "file").ToArray();
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
                App.Logger.Info($"Downloading {remoteFile.Name} ({(newFile ? "newFile" : "")}, {(fileChanged ? "fileChanged" : "")})");
                downloads.Add(Utilities.DownloadFile(remoteFile.Download, localFilePath));
            }

            // Remove HUD JSONs that aren't available online.
            foreach (var localFile in new DirectoryInfo("JSON").EnumerateFiles())
            {
                if (remoteFiles.Count((x) => x.Name == localFile.Name) != 0) continue;
                File.Delete(localFile.FullName);
                App.Logger.Info($"Removed {localFile.Name}");
            }

            await Task.WhenAll(downloads);
            return Convert.ToBoolean(downloads.Count);
        }
        catch (Exception e)
        {
            App.Logger.Error(e.Message);
            Console.WriteLine(e);
            return false;
        }
    }

    public async Task Add(string folderPath)
    {
        var hudName = folderPath.Split('\\')[^1];
        var hudDetailsFolder = $@"{Directory.CreateDirectory($@"JSON\Local\{hudName}").FullName}";
        var hudJson = new HudJson
        {
            Name = hudName,
            Thumbnail = Task.Run(() =>
            {
                var consoleFolder = $"{folderPath}\\materials\\console";
                var backgrounds = new[] { "2fort", "gravelpit", "mvm", "upward" };
                var backgroundSelection = backgrounds.FirstOrDefault(background => File.Exists($"{consoleFolder}\\background_{background}_widescreen.vtf"));
                if (backgroundSelection is null) return backgroundSelection;

                App.Logger.Info($"Found background file background_{backgroundSelection}_widescreen.vtf");
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
                    App.Logger.Info(process.StandardOutput.ReadLine());
                process.WaitForExit();
                process.Close();

                File.Move(outputPathTga, $"{outputPath}.tga", true);

                var tga = new TGA($"{outputPath}.tga");
                var rectImage = new System.Drawing.Bitmap((int)SystemParameters.PrimaryScreenWidth, (int)SystemParameters.PrimaryScreenHeight);
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