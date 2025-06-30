using Avalonia.Controls;
using CommunityToolkit.Mvvm.Input;
using HUDEditor.Assets;
using HUDEditor.Classes;
using HUDEditor.Models;
using System;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading.Tasks;

namespace HUDEditor.ViewModels;

public partial class SettingsViewModel : ViewModelBase
{
    public ObservableCollection<Language> Languages { get; } =
    [
        new() { CultureCode = "en-US", CultureName = Resources.ui_language_en, FlagImagePath = Utilities.LoadFromResource("avares://HUDEditor/Assets/Images/Flags/us.png") },
        new() { CultureCode = "fr-FR", CultureName = Resources.ui_language_fr, FlagImagePath = Utilities.LoadFromResource("avares://HUDEditor/Assets/Images/Flags/fr.png") },
        new() { CultureCode = "ru-RU", CultureName = Resources.ui_language_ru, FlagImagePath = Utilities.LoadFromResource("avares://HUDEditor/Assets/Images/Flags/ru.png") },
        new() { CultureCode = "pt-BR", CultureName = Resources.ui_language_pt, FlagImagePath = Utilities.LoadFromResource("avares://HUDEditor/Assets/Images/Flags/br.png") },
        new() { CultureCode = "it",    CultureName = Resources.ui_language_it, FlagImagePath = Utilities.LoadFromResource("avares://HUDEditor/Assets/Images/Flags/it.png") },
        new() { CultureCode = "zh-CN", CultureName = Resources.ui_language_cn, FlagImagePath = Utilities.LoadFromResource("avares://HUDEditor/Assets/Images/Flags/cn.png") },
    ];

    private Language _selectedLanguage;
    public Language SelectedLanguage
    {
        get => _selectedLanguage;
        set
        {
            _selectedLanguage = value;
            OnPropertyChanged(nameof(SelectedLanguage));
            SelectedCulture = value.CultureCode;
        }
    }
    public string SelectedCulture { get; private set; } = App.Config.ConfigSettings.UserPrefs.Language ?? "en-US";

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
        SelectedLanguage = Languages.FirstOrDefault(l => l.CultureCode == SelectedCulture);
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

        // Ask the user to restart the app if they've changed the language
        if (Resources.Culture != new CultureInfo(SelectedCulture)) Utilities.ShowMessageBox(Resources.info_ask_restart);
    }

    [RelayCommand]
    private async Task SetHudPath(Window window) => await Utilities.SetupDirectoryAsync(window, true);

    [RelayCommand]
    private void UpdateApp() => Utilities.UpdateAppSchema(false);

    [RelayCommand]
    private void OpenAppSettings() => Utilities.OpenWebpage($"{AppContext.BaseDirectory}/appsettings.json");

    [RelayCommand]
    private void OpenUserSettings() => Utilities.OpenWebpage($"{Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData)}/TF2HUD.Editor/settings.json");

    [RelayCommand]
    private async Task ClearAppCache() => await Utilities.ClearAppCache();
}