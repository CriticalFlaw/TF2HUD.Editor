using Avalonia.Controls;
using HUDEdit.Classes;
using System;
using System.Globalization;
using System.IO;
using System.Windows;

namespace HUDEdit.Views;

public partial class Settings : Avalonia.Controls.Window
{
    public Settings()
    {
        InitializeComponent();

        // Load the country flags
        LoadCountryFlagsAsync();

        // Check for user selected settings.
        BtnAutoUpdate.IsChecked = App.Config.ConfigSettings.UserPrefs.AutoUpdate;
        BtnPersistXhair.IsChecked = App.Config.ConfigSettings.UserPrefs.CrosshairPersistence;
    }

    private async void LoadCountryFlagsAsync()
    {
        ImgLocalizeEn.Source = await Utilities.LoadImageAsync("https://flagcdn.com/w320/us.png");
        ImgLocalizeFr.Source = await Utilities.LoadImageAsync("https://flagcdn.com/w320/fr.png");
        ImgLocalizeRu.Source = await Utilities.LoadImageAsync("https://flagcdn.com/w320/ru.png");
        ImgLocalizeBr.Source = await Utilities.LoadImageAsync("https://flagcdn.com/w320/br.png");
        ImgLocalizeIt.Source = await Utilities.LoadImageAsync("https://flagcdn.com/w320/it.png");
        ImgLocalizeCn.Source = await Utilities.LoadImageAsync("https://flagcdn.com/w320/cn.png");
    }

    /// <summary>
    /// Updates localization to the selected language.
    /// </summary>
    private void BtnLocalize_OnClick(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
    {
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

        // Save language preference to user settings.
        App.Config.ConfigSettings.UserPrefs.Language = Assets.Resources.Culture.ToString();
        App.SaveConfiguration();

        // Restart the application
        Utilities.RestartApplication();
    }

    private void SetDirectory_OnClick(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
    {
        App.Logger.Info("Attempting to change the target directory.");
        MainWindow.SetupDirectoryAsync(true);
    }

    private void BtnRefresh_OnClick(object? sender, Avalonia.Interactivity.RoutedEventArgs e) => MainWindow.UpdateAppSchema(false);

    private void BtnClearCache_Click(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
    {
        if (Utilities.ShowMessageBox(MessageBoxImage.Information, Assets.Resources.info_clear_cache, MessageBoxButton.YesNo) != MessageBoxResult.Yes) return;

        var localPath = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
        Directory.Delete($"{localPath}\\CriticalFlaw", true);
        Directory.Delete($"{localPath}\\TF2HUD.Editor", true);
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
}