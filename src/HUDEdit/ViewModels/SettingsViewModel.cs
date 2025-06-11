using Avalonia.Controls;
using Avalonia.Media.Imaging;
using Avalonia.Platform;
using CommunityToolkit.Mvvm.Input;
using HUDEdit.Assets;
using HUDEdit.Classes;
using HUDEdit.Models;
using System;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Globalization;
using System.Runtime.InteropServices;
using System.Threading.Tasks;

namespace HUDEdit.ViewModels;

public partial class SettingsViewModel : ViewModelBase
{
    public ObservableCollection<Language> Languages { get; } =
    [
        new() { CultureCode = "en-US", FlagImagePath = new Bitmap(AssetLoader.Open(new Uri("avares://HUDEdit/Assets/Images/Flags/us.png"))) },
        new() { CultureCode = "fr-FR", FlagImagePath = new Bitmap(AssetLoader.Open(new Uri("avares://HUDEdit/Assets/Images/Flags/fr.png"))) },
        new() { CultureCode = "ru-RU", FlagImagePath = new Bitmap(AssetLoader.Open(new Uri("avares://HUDEdit/Assets/Images/Flags/ru.png"))) },
        new() { CultureCode = "pt-BR", FlagImagePath = new Bitmap(AssetLoader.Open(new Uri("avares://HUDEdit/Assets/Images/Flags/br.png"))) },
        new() { CultureCode = "it",    FlagImagePath = new Bitmap(AssetLoader.Open(new Uri("avares://HUDEdit/Assets/Images/Flags/it.png"))) },
        new() { CultureCode = "zh-CN", FlagImagePath = new Bitmap(AssetLoader.Open(new Uri("avares://HUDEdit/Assets/Images/Flags/cn.png"))) },
    ];

    private string _selectedCulture;
    public string SelectedCulture
    {
        get => _selectedCulture;
        set
        {
            if (_selectedCulture != value)
            {
                _selectedCulture = value;
                OnPropertyChanged(nameof(SelectedCulture));

                // Apply culture
                Resources.Culture = new CultureInfo(value);
                App.Config.ConfigSettings.UserPrefs.Language = value;
                App.SaveConfiguration();
                Utilities.ShowMessageBox(Resources.info_ask_restart);
            }
        }
    }

    private bool _persistCrosshair;
    public bool PersistCrosshair
    {
        get => _persistCrosshair;
        set
        {
            _persistCrosshair = value;
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
            OnPropertyChanged(nameof(OverridePath));
        }
    }

    public SettingsViewModel()
    {
        SelectedCulture = App.Config.ConfigSettings.UserPrefs.Language ?? "en-US";
        PersistCrosshair = App.Config.ConfigSettings.UserPrefs.CrosshairPersistence;
        AutoUpdate = App.Config.ConfigSettings.UserPrefs.AutoUpdate;
        OverridePath = App.Config.ConfigSettings.UserPrefs.PathBypass;
    }

    [RelayCommand]
    private void SaveChanges()
    {
        App.Config.ConfigSettings.UserPrefs.Language = SelectedCulture;
        App.Config.ConfigSettings.UserPrefs.CrosshairPersistence = PersistCrosshair;
        App.Config.ConfigSettings.UserPrefs.AutoUpdate = AutoUpdate;
        App.Config.ConfigSettings.UserPrefs.PathBypass = OverridePath;
        App.SaveConfiguration();
    }

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