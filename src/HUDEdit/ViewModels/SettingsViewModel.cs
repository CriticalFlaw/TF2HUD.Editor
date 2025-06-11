using Avalonia.Controls;
using CommunityToolkit.Mvvm.Input;
using HUDEdit.Classes;
using System;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Threading.Tasks;

namespace HUDEdit.ViewModels;

public partial class SettingsViewModel : ViewModelBase
{
    private bool _persistCrosshair;
    public bool PersistCrosshair
    {
        get => _persistCrosshair;
        set
        {
            _persistCrosshair = value;
            App.Config.ConfigSettings.UserPrefs.CrosshairPersistence = _persistCrosshair;
            OnPropertyChanged(nameof(PersistCrosshair));
        }
    }

    private bool _autoUpdate;
    public bool AutoUpdate
    {
        get => _autoUpdate;
        set
        {
            _autoUpdate = value;
            App.Config.ConfigSettings.UserPrefs.AutoUpdate = _autoUpdate;
            OnPropertyChanged(nameof(AutoUpdate));
        }
    }

    private bool _overridePath;
    public bool OverridePath
    {
        get => _overridePath;
        set
        {
            _overridePath = value;
            App.Config.ConfigSettings.UserPrefs.PathBypass = _overridePath;
            OnPropertyChanged(nameof(OverridePath));
        }
    }

    public SettingsViewModel()
    {
        PersistCrosshair = App.Config.ConfigSettings.UserPrefs.CrosshairPersistence;
        AutoUpdate = App.Config.ConfigSettings.UserPrefs.AutoUpdate;
        OverridePath = App.Config.ConfigSettings.UserPrefs.PathBypass;
    }


    [RelayCommand]
    private void SaveChanges() => App.SaveConfiguration();

    [RelayCommand]
    private async Task SetHudPath(Window window) => await Utilities.SetupDirectoryAsync(window, true);

    [RelayCommand]
    private void UpdateApp() => Utilities.UpdateAppSchema(false);

    [RelayCommand]
    private void OpenAppSettings() => Process.Start(RuntimeInformation.IsOSPlatform(OSPlatform.Linux) ? "xdg-open" : "notepad.exe", $"{AppContext.BaseDirectory}/appsettings.json");

    [RelayCommand]
    private void OpenUserSettings() => Process.Start(RuntimeInformation.IsOSPlatform(OSPlatform.Linux) ? "xdg-open" : "notepad.exe", $"{Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData)}/TF2HUD.Editor/settings.json");

    [RelayCommand]
    private async Task ClearAppCache() => await Utilities.ClearAppCache();
}