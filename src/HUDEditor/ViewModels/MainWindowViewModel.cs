using Avalonia.Platform.Storage;
using CommunityToolkit.Mvvm.Input;
using Crews.Utility.TgaSharp;
using HUDEditor.Assets;
using HUDEditor.Classes;
using HUDEditor.Models;
using HUDEditor.Views;
using MsBox.Avalonia.Enums;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HUDEditor.ViewModels;

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
            InstallHUDCommand.NotifyCanExecuteChanged();
            UninstallHUDCommand.NotifyCanExecuteChanged();
        }
    }

    private string _windowTitle = Resources.ui_title;
    public string WindowTitle
    {
        get => _windowTitle;
        set
        {
            _windowTitle = value;
            OnPropertyChanged();
        }
    }

    private Avalonia.Controls.Window _mainWindow;
    public Avalonia.Controls.Window TopLevel
    {
        get => _mainWindow;
        set
        {
            _mainWindow = value;
            OnPropertyChanged();
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
    public void SelectHUD(HUD hud) => SelectedHud = hud;

    public bool CanInstall() => !Installing;

    public Task LoadHUDs()
    {
        _hudList = [];
        var sharedControlsJson = File.ReadAllText("JSON/shared-hud.json", new UTF8Encoding(false));

        foreach (var jsonFile in Directory.EnumerateFiles("JSON"))
        {
            var fileInfo = jsonFile.Replace("\\", "/").Split("/")[^1].Split(".");
            if (fileInfo[^1] != "json" || fileInfo[0] == "shared-hud") continue;

            if (fileInfo[0].Equals("common"))
            {
                var sharedHuds = JsonConvert.DeserializeObject<List<HudJson>>(File.ReadAllText("JSON/common.json", new UTF8Encoding(false)));
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
                _hudList.Add(new HUD(fileInfo[0], JsonConvert.DeserializeObject<HudJson>(File.ReadAllText(jsonFile, new UTF8Encoding(false))), true));
            }
        }

        foreach (var sharedHud in Directory.EnumerateDirectories(Directory.CreateDirectory(@"JSON/Local").FullName))
        {
            var hudName = Path.GetFileName(sharedHud.Replace("\\", "/"));
            var hudBackgroundPath = Path.Combine(sharedHud, "output.png");
            var hudBackground = File.Exists(hudBackgroundPath)
                ? $"file://{hudBackgroundPath}"
                : "avares://HUDEditor/Assets/Images/background.png";
            var sharedProperties = JsonConvert.DeserializeObject<HudJson>(sharedControlsJson);

            var hudJson = new HudJson
            {
                Name = hudName,
                Thumbnail = hudBackground,
                Background = hudBackground,
                Links = new Links { Update = $"file://{sharedHud}/{hudName}.zip" },
                Layout = sharedProperties.Layout,
                Controls = sharedProperties.Controls
            };

            _hudList.Add(new HUD(hudName, hudJson, false));
        }

        // Set current selection and viewmodel
        var selectedHud = this[App.Config.ConfigSettings.UserPrefs.SelectedHUD];
        _highlightedHud = selectedHud;
        _selectedHud = selectedHud;
        _currentPageViewModel = selectedHud != null ? new EditHUDViewModel(this, selectedHud) : new HomePageViewModel(this, HUDList);

        return Task.CompletedTask;
    }

    #region CLICK_EVENTS

    /// <summary>
    /// Invokes HUD installation or setting the tf/custom directory, if not already set.
    /// </summary>
    [RelayCommand(CanExecute = nameof(CanInstall))]
    public async void InstallHUD()
    {
        try
        {
            Installing = true;
            SelectedHud ??= HighlightedHud;

            // Force the user to set a directory before installing.
            if (!Utilities.CheckUserPath())
                if (await Utilities.SetupDirectoryAsync(TopLevel, true) == false) return;

            // Stop the process if Team Fortress 2 is still running.
            if (Utilities.CheckIsGameRunning())
            {
                Installing = false;
                return;
            }

            // Clear tf/custom directory of other installed HUDs.
            foreach (var x in HUDList)
            {
                if (Directory.Exists($"{App.HudPath}/{x.Name.ToLowerInvariant()}"))
                {
                    App.Logger.Info($"Removing {x.Name.ToLowerInvariant()} from {App.HudPath}");
                    Directory.Delete($"{App.HudPath}/{x.Name.ToLowerInvariant()}", true);
                }
            }

            // Check for unsupported HUDs in the tf/custom folder. Notify user if found.
            foreach (var foundHud in Directory.GetDirectories(App.HudPath))
            {
                if (!foundHud[App.HudPath.Length..].ToLowerInvariant().Contains("hud") || !File.Exists($"{foundHud}/info.vdf")) continue;
                if (await Utilities.ShowPromptBox(Resources.info_unsupported_hud_found) == ButtonResult.No)
                {
                    Installing = false;
                    return;
                }
                Directory.Delete(foundHud, true);
            }

            // Download and install the selected HUD
            await Utilities.DownloadHud(SelectedHud.DownloadUrl, App.HudPath, SelectedHud.Name);

            // Install Crosshairs
            if (SelectedHud.InstallCrosshairs)
            {
                App.Logger.Info($"Installing crosshairs to {SelectedHud.Name}");
                await Utilities.InstallCrosshairs($"{App.HudPath}/{SelectedHud.Name}");
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
            ((EditHUDViewModel)CurrentPageViewModel).Status = string.Format(Resources.status_installed_now, App.Config.ConfigSettings.UserPrefs.SelectedHUD, DateTime.Now);

            // Update Menu Buttons
            OnPropertyChanged(nameof(HighlightedHudInstalled));
            OnPropertyChanged(nameof(SelectedHud));
            OnPropertyChanged(nameof(SelectedHudInstalled));
            Installing = false;
        }
        catch (Exception e)
        {
            await Utilities.ShowMessageBox($"{string.Format(Resources.error_hud_install, SelectedHud.Name)} {e.Message}", MsBox.Avalonia.Enums.Icon.Error);
            Installing = false;
        }
    }

    /// <summary>
    /// Invokes HUD deletion from the tf/custom directory.
    /// </summary>
    [RelayCommand]
    public void UninstallHUD()
    {
        try
        {
            // Check if the HUD is installed in a valid directory.
            if (!SelectedHudInstalled) return;

            // Stop the process if Team Fortress 2 is still running.
            if (Utilities.CheckIsGameRunning()) return;

            // Remove the HUD from the tf/custom directory.
            App.Logger.Info($"Removing {SelectedHud.Name} from {App.HudPath}");
            if (SelectedHud.Name != "") Directory.Delete($"{App.HudPath}/{SelectedHud.Name}", true);

            // Update timestamp
            ((EditHUDViewModel)CurrentPageViewModel).Status = string.Format(Resources.status_installed_not, App.Config.ConfigSettings.UserPrefs.SelectedHUD, DateTime.Now);

            // Update Menu Buttons
            OnPropertyChanged(nameof(HighlightedHud));
            OnPropertyChanged(nameof(HighlightedHudInstalled));
            OnPropertyChanged(nameof(SelectedHud));
            OnPropertyChanged(nameof(SelectedHudInstalled));
        }
        catch (Exception e)
        {
            Utilities.ShowMessageBox($"{string.Format(Resources.error_hud_uninstall, SelectedHud.Name)} {e.Message}", MsBox.Avalonia.Enums.Icon.Error);
        }
    }

    /// <summary>
    /// Saves and applies user settings to the HUD files.
    /// </summary>
    [RelayCommand]
    public async Task SaveHUD()
    {
        if (SelectedHud == null) return;

        var selection = SelectedHud;
        if ((Process.GetProcessesByName("hl2").Any() || Process.GetProcessesByName("tf").Any() || Process.GetProcessesByName("tf_win64").Any()) && selection.DirtyControls.Count > 0)
        {
            var message = selection.DirtyControls.Aggregate(Resources.info_game_restart, (current, control) => current + $"\n - {control}");
            if (await Utilities.ShowPromptBox(message) == ButtonResult.No) return;
        }

        App.Logger.Info("------");
        App.Logger.Info("Applying user settings");
        selection.Settings.SaveSettings();
        selection.ApplyCustomizations();
        selection.DirtyControls.Clear();

        ((EditHUDViewModel)CurrentPageViewModel).Status = string.Format(Resources.status_applied, selection.Name, DateTime.Now);
    }

    /// <summary>
    /// Resets user settings for the selected HUD to their default values.
    /// </summary>
    [RelayCommand]
    public async void ResetHUD()
    {
        // Ask the user if they want to reset before doing so.
        if (await Utilities.ShowPromptBox(Resources.info_hud_reset) == ButtonResult.No) return;

        App.Logger.Info("------");
        App.Logger.Info("Resetting user settings");
        var selection = SelectedHud;
        selection.ResetAll();
        selection.Settings.SaveSettings();
        selection.ApplyCustomizations();
        selection.DirtyControls.Clear();
        ((EditHUDViewModel)CurrentPageViewModel).Status = string.Format(Resources.status_reset, selection.Name, DateTime.Now);
    }

    /// <summary>
    /// Returns to the HUD selection page.
    /// </summary>
    [RelayCommand]
    public void SwitchHUD()
    {
        App.Logger.Info("Changing page view to: main menu");
        HighlightedHud = null;
        SelectedHud = null;
        WindowTitle = Resources.ui_title;
    }

    [RelayCommand]
    public void OpenDocSite() => Utilities.OpenWebpage(App.Config.ConfigSettings.AppConfig.DocumentationURL);

    [RelayCommand]
    public void OpenIssueTracker() => Utilities.OpenWebpage(App.Config.ConfigSettings.AppConfig.IssueTrackerURL);

    [RelayCommand]
    public void OpenOptionsMenu() => new SettingsView().Show();

    [RelayCommand]
    public void LaunchTf2() => Utilities.OpenWebpage("steam://run/440");

    /// <summary>
    /// Adds a HUD from folder to the shared HUDs list.
    /// </summary>
    [RelayCommand]
    public async void AddSharedHud()
    {
        try
        {
            if (await Utilities.ShowPromptBox(Resources.info_add_hud) == ButtonResult.No) return;

            var folders = await TopLevel.StorageProvider.OpenFolderPickerAsync(new FolderPickerOpenOptions
            {
                Title = Resources.info_path_browser,
                AllowMultiple = false
            });

            if (folders.Count <= 0) return;
            await Add(folders[0].TryGetLocalPath());
        }
        catch (Exception e)
        {
            Utilities.ShowMessageBox(e.Message, MsBox.Avalonia.Enums.Icon.Error);
        }
    }

    [RelayCommand]
    public async void RefreshPage()
    {
        // Dispose of the old viewmodel
        CurrentPageViewModel?.Dispose();

        await LoadHUDs();
        await DownloadImages();
        OnPropertyChanged(nameof(HUDList));

        // Restore selected HUD if it still exists
        SelectedHud = this[SelectedHud?.Name];

        App.Logger.Info($"Refreshed HUD list and reloaded page: {SelectedHud?.Name ?? "home"} page");
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
            List<Task> downloads = [];

            foreach (var remoteFile in remoteFiles)
            {
                var localFilePath = $"JSON/{remoteFile.Name}";
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
        folderPath = folderPath.Replace("\\", "/");
        var hudName = Path.GetFileName(folderPath);
        var hudDetailsFolder = $@"{Directory.CreateDirectory($@"JSON/Local/{hudName}").FullName}".Replace("\\", "/");
        var thumbnail = await GenerateThumbnailAsync(folderPath, hudDetailsFolder);
        var updateLink = await Utilities.CreateHudZipAsync(folderPath, hudDetailsFolder, hudName);
        var sharedControlsJson = new StreamReader(File.OpenRead("JSON/shared-hud.json"), new UTF8Encoding(false)).ReadToEnd();
        var hudControls = JsonConvert.DeserializeObject<HudJson>(sharedControlsJson);
        foreach (var group in hudControls.Controls)
        {
            foreach (var control in hudControls.Controls[group.Key])
                control.Name = $"{Utilities.EncodeId(hudName)}_{Utilities.EncodeId(control.Name)}";
        }

        // Install the crosshairs
        await Utilities.InstallCrosshairs(folderPath);

        var hudJson = new HudJson
        {
            Name = hudName,
            Thumbnail = thumbnail,
            Background = thumbnail,
            Links = new Links { Update = updateLink },
            Layout = hudControls.Layout,
            Controls = hudControls.Controls
        };

        var hud = new HUD(hudName, hudJson, false);
        _hudList.Add(hud);
        _hudList.Sort((a, b) => string.CompareOrdinal(a.Name, b.Name));
        SelectedHud = hud;
    }

    private async Task<string?> GenerateThumbnailAsync(string folderPath, string hudDetailsFolder)
    {
        var consoleFolder = $"{folderPath}/materials/console";
        var backgrounds = new[] { "2fort", "gravelpit", "mvm", "upward" };
        var backgroundSelection = backgrounds.FirstOrDefault(background => File.Exists($"{consoleFolder}/background_{background}_widescreen.vtf"));
        if (backgroundSelection is null) return backgroundSelection;

        App.Logger.Info($"Found background file background_{backgroundSelection}_widescreen.vtf");
        var inputPath = $"{consoleFolder}/background_{backgroundSelection}_widescreen.vtf";
        var outputPathTga = $"{consoleFolder}/output.tga";
        var outputPath = $"{hudDetailsFolder}/output";

        string[] args =
        [
            "-i",
                    $"\"{inputPath}\"",
                    "-o",
                    $"\"{outputPathTga}\""
        ];

        var workingDir = $"{App.HudPath.Replace("/tf/custom", string.Empty)}/bin";
        var exePath = Path.Combine(workingDir, "vtf2tga.exe");
        var processInfo = new ProcessStartInfo
        {
            FileName = exePath,
            Arguments = string.Join(" ", args),
            WorkingDirectory = workingDir,
            UseShellExecute = false,
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            CreateNoWindow = true
        };

        if (!File.Exists(exePath)) throw new FileNotFoundException("VTF2TGA not found at expected location.", exePath);

        var process = Process.Start(processInfo);
        while (!process.StandardOutput.EndOfStream)
            App.Logger.Info(process.StandardOutput.ReadLine());
        process.WaitForExit();
        process.Close();

        File.Move(outputPathTga, $"{outputPath}.tga", true);

        var tga = new TGA($"{outputPath}.tga");
        var rectImage = new Bitmap(1920, 1080);
        var graphics = Graphics.FromImage(rectImage);
        graphics.DrawImage((Image)tga, 0, 0, rectImage.Width, rectImage.Height);
        rectImage.Save($"{outputPath}.png");

        return $"file://{outputPath}.png";
    }

    public async Task DownloadImages()
    {
        foreach (var hud in _hudList)
        {
            hud.ThumbnailImage = await ImageCache.GetImageAsync(hud.Thumbnail);

            // Load and cache screenshots
            hud.ScreenshotImages = [];
            if (hud.Screenshots != null)
            {
                foreach (var screenshotUrl in hud.Screenshots)
                {
                    var img = await ImageCache.GetImageAsync(screenshotUrl);
                    if (img != null)
                        hud.ScreenshotImages.Add(img);
                }
            }
        }
    }
}