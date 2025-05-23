using HUDEdit.Classes;
using MsBox.Avalonia.Enums;
using System;
using System.Diagnostics;
using System.Globalization;
using System.IO;

namespace HUDEdit.Views;

public partial class SettingsView : Avalonia.Controls.Window
{
    private bool _isInitializing = true;

    public SettingsView()
    {
        InitializeComponent();

        // Set the flag so the event handler knows we're still initializing
        _isInitializing = true;

        // Set the radio button based on selected user language.
        switch (App.Config.ConfigSettings.UserPrefs.Language)
        {
            case "fr-FR":
                BtnLocalizeFr.IsChecked = true;
                break;
            case "ru-RU":
                BtnLocalizeRu.IsChecked = true;
                break;
            case "pt-BR":
                BtnLocalizeBr.IsChecked = true;
                break;
            case "it":
                BtnLocalizeIt.IsChecked = true;
                break;
            case "zh-CN":
                BtnLocalizeCn.IsChecked = true;
                break;
            default:
                BtnLocalizeEn.IsChecked = true;
                break;
        }

        BtnAutoUpdate.IsChecked = App.Config.ConfigSettings.UserPrefs.AutoUpdate;
        BtnPersistXhair.IsChecked = App.Config.ConfigSettings.UserPrefs.CrosshairPersistence;

        _isInitializing = false;
    }

    /// <summary>
    /// Updates localization to the selected language.
    /// </summary>
    private void BtnLocalize_OnClick(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
    {
        // Ignore clicks during initialization
        if (_isInitializing) return;

        if (BtnLocalizeEn.IsChecked == true)
            Assets.Resources.Culture = new CultureInfo("en-US");
        else if (BtnLocalizeFr.IsChecked == true)
            Assets.Resources.Culture = new CultureInfo("fr-FR");
        else if (BtnLocalizeRu.IsChecked == true)
            Assets.Resources.Culture = new CultureInfo("ru-RU");
        else if (BtnLocalizeBr.IsChecked == true)
            Assets.Resources.Culture = new CultureInfo("pt-BR");
        else if (BtnLocalizeIt.IsChecked == true)
            Assets.Resources.Culture = new CultureInfo("it");
        else if (BtnLocalizeCn.IsChecked == true)
            Assets.Resources.Culture = new CultureInfo("zh-CN");

        // Save language preference then restart.
        App.Config.ConfigSettings.UserPrefs.Language = Assets.Resources.Culture.ToString();
        App.SaveConfiguration();
        Utilities.RestartApplication();
    }

    private async void SetDirectory_OnClick(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
    {
        App.Logger.Info("Attempting to change the target directory.");
        await Utilities.SetupDirectoryAsync(this, true);
    }

    private void BtnRefresh_OnClick(object? sender, Avalonia.Interactivity.RoutedEventArgs e) => MainWindow.UpdateAppSchema(false);

    private async void BtnClearCache_Click(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
    {
        if (await Utilities.ShowPromptBox(Assets.Resources.info_clear_cache) == ButtonResult.No) return;

        Directory.Delete($"{Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData)}\\TF2HUD.Editor", true);
        Directory.Delete("JSON", true);
        MainWindow.UpdateAppSchema(true);
    }

    private void BtnPersistXhair_Click(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
    {
        App.Config.ConfigSettings.UserPrefs.CrosshairPersistence = BtnAutoUpdate.IsChecked ?? true;
        App.SaveConfiguration();
    }

    private void BtnAutoUpdate_OnClick(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
    {
        App.Config.ConfigSettings.UserPrefs.AutoUpdate = BtnAutoUpdate.IsChecked ?? true;
        App.SaveConfiguration();
    }

    private void BtnOpenAppSettings_Click(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
    {
        Process.Start("notepad.exe", $"{AppContext.BaseDirectory}\\appsettings.json");
    }

    private void BtnOpenUserSettings_Click(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
    {
        Process.Start("notepad.exe", $"{Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData)}\\TF2HUD.Editor\\settings.json");
    }
}