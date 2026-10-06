using Avalonia.Platform.Storage;
using CommunityToolkit.Mvvm.Input;
using HUDEditor.Assets;
using HUDEditor.Classes;
using HUDEditor.Models;
using HUDEditor.Views;
using MsBox.Avalonia.Enums;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HUDEditor.ViewModels;

public partial class MainWindowViewModel : ViewModelBase
{
    private List<HUD> _hudList = [];
    public IEnumerable<HUD> HUDList => _hudList;
    private HUD? _highlightedHud;
    public HUD? HighlightedHud
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
    private HUD? _selectedHud;
    public HUD? SelectedHud
    {
        get => _selectedHud;
        private set
        {
            _highlightedHud = null;
            _selectedHud = value;
            OnPropertyChanged(nameof(SelectedHud));
            OnPropertyChanged(nameof(SelectedHudInstalled));

            CurrentPageViewModel?.Dispose();
            CurrentPageViewModel = _selectedHud is { } hud ? new EditHUDViewModel(this, hud) : new HomePageViewModel(this, HUDList);
            App.Logger.Info($"Changing page view to: {(_selectedHud?.Name ?? "Home")}");
            App.Config.ConfigSettings.UserPrefs.SelectedHUD = SelectedHud?.Name ?? string.Empty;
        }
    }

    public bool SelectedHudInstalled => Utilities.CheckHudInstallation(SelectedHud);
    private ViewModelBase? _currentPageViewModel;
    public ViewModelBase? CurrentPageViewModel
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

    private Avalonia.Controls.Window? _mainWindow;
    public Avalonia.Controls.Window? TopLevel
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
    public HUD? this[string name] => HUDList.FirstOrDefault(hud => string.Equals(hud.Name, name, StringComparison.InvariantCultureIgnoreCase));

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
        App.SaveConfiguration();
    }

    public bool CanInstall() => !Installing;

    public async Task LoadHUDs()
    {
        _hudList = [];
        var jsonFolder = Utilities.JsonFolder;
        var sharedHudFile = Path.Combine(jsonFolder, "shared-hud.json");
        if (!File.Exists(sharedHudFile)) await Utilities.UpdateAppSchema();
        if (!File.Exists(sharedHudFile))
            throw new FileNotFoundException("HUD schema files are missing and could not be downloaded. Check your internet connection and restart the app.", sharedHudFile);
        var sharedControlsJson = await File.ReadAllTextAsync(sharedHudFile, new UTF8Encoding(false));

        foreach (var jsonFile in Directory.EnumerateFiles(jsonFolder, "*.json"))
        {
            var fileName = Path.GetFileNameWithoutExtension(jsonFile);
            if (fileName == "shared-hud") continue;

            // One malformed schema shouldn't prevent the editor from starting.
            try
            {
                if (fileName.Equals("common"))
                {
                    var sharedHuds = JsonConvert.DeserializeObject<List<HudJson>>(File.ReadAllText(jsonFile, new UTF8Encoding(false))) ?? [];
                    foreach (var sharedHud in sharedHuds)
                    {
                        var hudControls = JsonConvert.DeserializeObject<HudJson>(sharedControlsJson)!;
                        foreach (var control in hudControls.Controls.SelectMany(group => hudControls.Controls[group.Key]))
                            control.Name = $"{Utilities.EncodeId(sharedHud.Name)}_{Utilities.EncodeId(control.Name)}";
                        sharedHud.Layout = hudControls.Layout;
                        sharedHud.Controls = hudControls.Controls;
                        _hudList.Add(new HUD(sharedHud.Name, sharedHud, false));
                    }
                }
                else
                {
                    var schema = JsonConvert.DeserializeObject<HudJson>(File.ReadAllText(jsonFile, new UTF8Encoding(false)));
                    if (schema is not null) _hudList.Add(new HUD(fileName, schema, true));
                }
            }
            catch (Exception e)
            {
                App.Logger.Error($"Failed to load HUD schema \"{jsonFile}\": {e.Message}");
            }
        }

        foreach (var sharedHud in Directory.EnumerateDirectories(Directory.CreateDirectory(Path.Combine(jsonFolder, "Local")).FullName))
        {
            var hudName = Path.GetFileName(sharedHud);
            var zipPath = Path.Combine(sharedHud, $"{hudName}.zip");
            if (!File.Exists(zipPath))
            {
                App.Logger.Warn($"Skipping local HUD \"{hudName}\": archive not found at \"{zipPath}\".");
                continue;
            }

            var hudBackgroundPath = Path.Combine(sharedHud, "output.png");
            var hudBackground = File.Exists(hudBackgroundPath)
                ? new Uri(hudBackgroundPath).AbsoluteUri
                : "avares://HUDEditor/Assets/Images/background.png";
            _hudList.Add(CreateLocalHud(hudName, hudBackground, new Uri(zipPath).AbsoluteUri, sharedControlsJson));
        }

        // Set current selection and viewmodel
        var selectedHud = this[App.Config.ConfigSettings.UserPrefs.SelectedHUD];
        _highlightedHud = selectedHud;
        _selectedHud = selectedHud;
        _currentPageViewModel = selectedHud != null ? new EditHUDViewModel(this, selectedHud) : new HomePageViewModel(this, HUDList);
    }

    /// <summary>
    /// Builds a HUD object for a HUD added from a local folder, using the shared HUD controls.
    /// </summary>
    private static HUD CreateLocalHud(string hudName, string background, string updateLink, string sharedControlsJson)
    {
        var hudControls = JsonConvert.DeserializeObject<HudJson>(sharedControlsJson)!;
        foreach (var control in hudControls.Controls.SelectMany(group => group.Value))
            control.Name = $"{Utilities.EncodeId(hudName)}_{Utilities.EncodeId(control.Name)}";

        var hudJson = new HudJson
        {
            Name = hudName,
            Thumbnail = background,
            Background = background,
            Links = new Links { Update = updateLink },
            Layout = hudControls.Layout,
            Controls = hudControls.Controls,
            // Install the crosshair pack into the installed copy, not the user's source folder.
            InstallCrosshairs = true
        };

        return new HUD(hudName, hudJson, false);
    }

    /// <summary>
    /// Loads thumbnail and screenshot images for HUDs that don't have them yet (uses the on-disk cache).
    /// </summary>
    public async Task LoadHudImagesAsync()
    {
        foreach (var hud in _hudList.Where(x => x.ThumbnailImage is null))
        {
            if (!string.IsNullOrWhiteSpace(hud.Thumbnail))
                hud.ThumbnailImage = await ImageCache.GetImageAsync(hud.Thumbnail);

            hud.ScreenshotImages = [];
            foreach (var screenshot in hud.Screenshots)
            {
                var image = await ImageCache.GetImageAsync(screenshot);
                if (image is not null) hud.ScreenshotImages.Add(image);
            }
        }
    }

    #region CLICK_EVENTS

    /// <summary>
    /// Invokes HUD installation or setting the tf/custom directory, if not already set.
    /// </summary>
    [RelayCommand]
    public async Task InstallHUD()
    {
        try
        {
            Installing = true;

            SelectedHud ??= HighlightedHud;
            var hud = SelectedHud;
            if (hud is null) return;

            // Force the user to set a directory before installing.
            if (!Utilities.CheckUserPath())
                if (TopLevel is null || await Utilities.SetupDirectoryAsync(TopLevel, true) == false) return;

            // Stop the process if Team Fortress 2 is still running.
            if (await Utilities.CheckIsGameRunning())
            {
                Installing = false;
                return;
            }

            // Download first, so a failed download doesn't leave the user without a HUD.
            var hudArchive = await Utilities.DownloadHudArchive(hud.DownloadUrl, hud.Name);

            // Check for unsupported HUDs in the tf/custom folder. Notify user if found.
            var knownHuds = HUDList.Select(x => x.Name).ToHashSet(StringComparer.OrdinalIgnoreCase);
            foreach (var foundHud in Directory.GetDirectories(App.HudPath))
            {
                var folderName = Path.GetFileName(foundHud);
                if (knownHuds.Contains(folderName)) continue;
                if (!folderName.Contains("hud", StringComparison.OrdinalIgnoreCase) || !File.Exists(Path.Combine(foundHud, "info.vdf"))) continue;
                if (await Utilities.ShowPromptBox(Resources.info_unsupported_hud_found) == ButtonResult.No)
                    return;
                Utilities.DeleteDirectory(foundHud);
            }

            // Clear tf/custom directory of other installed HUDs (match case-insensitively for Linux).
            foreach (var foundHud in Directory.GetDirectories(App.HudPath))
            {
                if (!knownHuds.Contains(Path.GetFileName(foundHud))) continue;
                App.Logger.Info($"Removing {foundHud}");
                Utilities.DeleteDirectory(foundHud);
            }

            // Install the selected HUD
            Utilities.ExtractHud(hudArchive, App.HudPath, hud.Name);

            // Install Crosshairs
            if (hud.InstallCrosshairs)
            {
                App.Logger.Info($"Installing crosshairs to {hud.Name}");
                await Utilities.InstallCrosshairs($"{App.HudPath}/{hud.Name}");
            }

            // Update the page view.
            if (string.IsNullOrWhiteSpace(hud.Name))
            {
                Installing = false;
                return;
            }
            hud.Settings.SaveSettings();
            if (!hud.ApplyCustomizations())
                await Utilities.ShowMessageBox(Resources.error_hud_apply_partial, MsBox.Avalonia.Enums.Icon.Warning);

            // Update timestamp
            if (CurrentPageViewModel is EditHUDViewModel editVm)
                editVm.Status = string.Format(Resources.status_installed_now, App.Config.ConfigSettings.UserPrefs.SelectedHUD, DateTime.Now);

            // Update Menu Buttons
            OnPropertyChanged(nameof(HighlightedHudInstalled));
            OnPropertyChanged(nameof(SelectedHud));
            OnPropertyChanged(nameof(SelectedHudInstalled));
        }
        catch (Exception e)
        {
            await Utilities.ShowMessageBox($"{string.Format(Resources.error_hud_install, SelectedHud?.Name)} {e.Message}", MsBox.Avalonia.Enums.Icon.Error);
        }
        finally
        {
            Installing = false;
        }
    }

    /// <summary>
    /// Invokes HUD deletion from the tf/custom directory.
    /// </summary>
    [RelayCommand]
    public async Task UninstallHUD()
    {
        try
        {
            // Check if the HUD is installed in a valid directory.
            var hud = SelectedHud;
            if (hud is null || !SelectedHudInstalled) return;

            // Stop the process if Team Fortress 2 is still running.
            if (await Utilities.CheckIsGameRunning()) return;

            // Remove the HUD from the tf/custom directory.
            App.Logger.Info($"Removing {hud.Name} from {App.HudPath}");
            if (hud.Name != "") Utilities.DeleteDirectory($"{App.HudPath}/{hud.Name}");

            // Update timestamp
            if (CurrentPageViewModel is EditHUDViewModel editVm)
                editVm.Status = string.Format(Resources.status_installed_not, App.Config.ConfigSettings.UserPrefs.SelectedHUD, DateTime.Now);

            // Update Menu Buttons
            OnPropertyChanged(nameof(HighlightedHud));
            OnPropertyChanged(nameof(HighlightedHudInstalled));
            OnPropertyChanged(nameof(SelectedHud));
            OnPropertyChanged(nameof(SelectedHudInstalled));
        }
        catch (Exception e)
        {
            await Utilities.ShowMessageBox($"{string.Format(Resources.error_hud_uninstall, SelectedHud?.Name)} {e.Message}", MsBox.Avalonia.Enums.Icon.Error);
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
        var applied = selection.ApplyCustomizations();
        selection.DirtyControls.Clear();
        if (!applied) await Utilities.ShowMessageBox(Resources.error_hud_apply_partial, MsBox.Avalonia.Enums.Icon.Warning);

        if (CurrentPageViewModel is EditHUDViewModel editVm)
            editVm.Status = string.Format(Resources.status_applied, selection.Name, DateTime.Now);
    }

    /// <summary>
    /// Resets user settings for the selected HUD to their default values.
    /// </summary>
    [RelayCommand]
    public async Task ResetHUD()
    {
        if (SelectedHud == null) return;

        // Ask the user if they want to reset before doing so.
        if (await Utilities.ShowPromptBox(Resources.info_hud_reset) == ButtonResult.No) return;

        App.Logger.Info("------");
        App.Logger.Info($"Resetting {SelectedHud.Name} settings");
        var selection = SelectedHud;
        selection.ResetAll();
        selection.Settings.SaveSettings();
        var applied = !Utilities.CheckHudInstallation(selection) || selection.ApplyCustomizations();
        selection.DirtyControls.Clear();
        if (!applied) await Utilities.ShowMessageBox(Resources.error_hud_apply_partial, MsBox.Avalonia.Enums.Icon.Warning);

        if (CurrentPageViewModel is EditHUDViewModel editVm)
            editVm.Status = string.Format(Resources.status_reset, selection.Name, DateTime.Now);
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
        App.SaveConfiguration();
    }

    [RelayCommand]
    public async Task OpenDocSite() => await Utilities.OpenWebpage(App.Config.ConfigSettings.AppConfig.DocumentationURL);

    [RelayCommand]
    public async Task OpenIssueTracker() => await Utilities.OpenWebpage(App.Config.ConfigSettings.AppConfig.IssueTrackerURL);

    [RelayCommand]
    public void OpenOptionsMenu() => new SettingsView().Show();

    [RelayCommand]
    public async Task LaunchTf2() => await Utilities.OpenWebpage("steam://run/440");

    /// <summary>
    /// Adds a HUD from folder to the shared HUDs list.
    /// </summary>
    [RelayCommand]
    public async Task AddSharedHud()
    {
        try
        {
            if (TopLevel is null || await Utilities.ShowPromptBox(Resources.info_add_hud) == ButtonResult.No) return;

            var folders = await TopLevel.StorageProvider.OpenFolderPickerAsync(new FolderPickerOpenOptions
            {
                Title = Resources.info_path_browser,
                AllowMultiple = false
            });

            if (folders.Count <= 0) return;
            var localPath = folders[0].TryGetLocalPath();
            if (localPath is null)
            {
                await Utilities.ShowMessageBox(Resources.info_path_invalid, MsBox.Avalonia.Enums.Icon.Error);
                return;
            }
            await Add(localPath);
        }
        catch (Exception e)
        {
            await Utilities.ShowMessageBox(e.Message, MsBox.Avalonia.Enums.Icon.Error);
        }
    }

    [RelayCommand]
    public async Task RefreshPage()
    {
        var previousPage = CurrentPageViewModel;
        await LoadHUDs();
        previousPage?.Dispose();
        await LoadHudImagesAsync();
        OnPropertyChanged(nameof(HUDList));

        // Restore the selected HUD if it still exists. Read the selection now rather than before reloading,
        // since the user may have switched pages while images were loading.
        var selection = App.Config.ConfigSettings.UserPrefs.SelectedHUD;
        SelectedHud = string.IsNullOrEmpty(selection) ? null : this[selection];

        App.Logger.Info($"Refreshed HUD list and reloaded page: {SelectedHud?.Name ?? "home"} page");
    }

    #endregion CLICK_EVENTS

    public async Task Add(string folderPath)
    {
        folderPath = Path.GetFullPath(folderPath).TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar);
        var hudName = Path.GetFileName(folderPath);

        if (this[hudName] is not null)
        {
            await Utilities.ShowMessageBox(string.Format(Resources.error_hud_exists, hudName), MsBox.Avalonia.Enums.Icon.Warning);
            return;
        }

        var hudDetailsFolder = Directory.CreateDirectory(Path.Combine(Utilities.JsonFolder, "Local", hudName)).FullName;
        try
        {
            var thumbnail = await GenerateThumbnailAsync(folderPath, hudDetailsFolder);
            var updateLink = await Utilities.CreateHudZipAsync(folderPath, hudDetailsFolder, hudName);
            var sharedControlsJson = await File.ReadAllTextAsync(Path.Combine(Utilities.JsonFolder, "shared-hud.json"), new UTF8Encoding(false));

            var hud = CreateLocalHud(hudName, thumbnail ?? "avares://HUDEditor/Assets/Images/background.png", updateLink, sharedControlsJson);
            if (thumbnail is not null) hud.ThumbnailImage = await ImageCache.GetImageAsync(thumbnail);

            _hudList.Add(hud);
            _hudList.Sort((a, b) => string.CompareOrdinal(a.Name, b.Name));
            SelectedHud = hud;
            App.SaveConfiguration();
        }
        catch
        {
            // Don't leave a half-added HUD behind; it would show up in the list on the next launch.
            Utilities.DeleteDirectory(hudDetailsFolder);
            throw;
        }
    }

    /// <summary>
    /// Generates a thumbnail from the HUD's main menu background, if it has one. Returns null on failure.
    /// </summary>
    private static async Task<string?> GenerateThumbnailAsync(string folderPath, string hudDetailsFolder)
    {
        var consoleFolder = Path.Combine(folderPath, "materials", "console");
        var backgrounds = new[] { "2fort", "gravelpit", "mvm", "upward" };
        var backgroundSelection = backgrounds.FirstOrDefault(background => File.Exists(Path.Combine(consoleFolder, $"background_{background}_widescreen.vtf")));
        if (backgroundSelection is null) return null;

        App.Logger.Info($"Found background file background_{backgroundSelection}_widescreen.vtf");
        var inputPath = Path.Combine(consoleFolder, $"background_{backgroundSelection}_widescreen.vtf");
        var extractedPath = Path.Combine(hudDetailsFolder, "extracted.png");
        var outputPath = Path.Combine(hudDetailsFolder, "output.png");

        try
        {
            await Task.Run(() =>
            {
                VTF.ExtractToPng(inputPath, extractedPath);
                VTF.ResizeImage(extractedPath, 1920, 1080, outputPath);
                File.Delete(extractedPath);
            });
            return new Uri(outputPath).AbsoluteUri;
        }
        catch (Exception e)
        {
            App.Logger.Warn($"Could not generate a thumbnail for {folderPath}: {e.Message}");
            return null;
        }
    }
}