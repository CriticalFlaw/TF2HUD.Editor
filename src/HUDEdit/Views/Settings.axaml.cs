using Avalonia.Media.Imaging;
using System.Globalization;
using System.IO;
using System.Windows;
using System;
using WPFLocalizeExtension.Engine;

namespace HUDEdit.Views;

public partial class Settings : Avalonia.Controls.Window
{
    public Settings()
    {
        InitializeComponent();

        // Load the country flags
        ImgLocalizeEn.Source = new Bitmap("https://flagcdn.com/w320/us.png");
        ImgLocalizeFr.Source = new Bitmap("https://flagcdn.com/w320/fr.png");
        ImgLocalizeRu.Source = new Bitmap("https://flagcdn.com/w320/ru.png");
        ImgLocalizeBr.Source = new Bitmap("https://flagcdn.com/w320/br.png");
        ImgLocalizeIt.Source = new Bitmap("https://flagcdn.com/w320/it.png");
        ImgLocalizeCn.Source = new Bitmap("https://flagcdn.com/w320/cn.png");

        // Check for user selected settings.
        BtnAutoUpdate.IsChecked = App.Config.ConfigSettings.UserPrefs.AutoUpdate;
        BtnPersistXhair.IsChecked = App.Config.ConfigSettings.UserPrefs.CrosshairPersistence;
    }

    /// <summary>
    ///     Updates localization to the selected language.
    /// </summary>
    private void BtnLocalize_OnClick(object sender, RoutedEventArgs e)
    {
        if (BtnLocalizeEn.IsChecked == true)
            LocalizeDictionary.Instance.Culture = new CultureInfo("en-US");
        else if (BtnLocalizeFr.IsChecked == true)
            LocalizeDictionary.Instance.Culture = new CultureInfo("fr-FR");
        else if (BtnLocalizeRu.IsChecked == true)
            LocalizeDictionary.Instance.Culture = new CultureInfo("ru-RU");
        else if (BtnLocalizeBr.IsChecked == true)
            LocalizeDictionary.Instance.Culture = new CultureInfo("pt-BR");
        else if (BtnLocalizeIt.IsChecked == true)
            LocalizeDictionary.Instance.Culture = new CultureInfo("it");
        else if (BtnLocalizeCn.IsChecked == true)
            LocalizeDictionary.Instance.Culture = new CultureInfo("zh-CN");

        // Save language preference to user settings.
        App.Config.ConfigSettings.UserPrefs.Language = LocalizeDictionary.Instance.Culture.ToString();
        App.SaveConfiguration();
    }

    private void BtnSetDirectory_OnClick(object sender, RoutedEventArgs e)
    {
        App.Logger.Info("Attempting to change the target directory.");
        MainWindow.SetupDirectory(true);
    }

    private void BtnRefresh_OnClick(object sender, RoutedEventArgs e)
    {
        MainWindow.UpdateAppSchema(false);
    }

    private void BtnAutoUpdate_OnClick(object sender, RoutedEventArgs e)
    {
        App.Config.ConfigSettings.UserPrefs.AutoUpdate = BtnAutoUpdate.IsChecked ?? true;
        App.SaveConfiguration();
    }

    private void BtnPersistXhair_Click(object sender, RoutedEventArgs e)
    {
        App.Config.ConfigSettings.UserPrefs.CrosshairPersistence = BtnPersistXhair.IsChecked ?? true;
        App.SaveConfiguration();
    }

    private void btnClearCache_Click(object sender, RoutedEventArgs e)
    {
        if (MainWindow.ShowMessageBox(MessageBoxImage.Information, Localization.Resources.info_clear_cache, MessageBoxButton.YesNo) != MessageBoxResult.Yes) return;

        var localPath = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
        Directory.Delete($"{localPath}\\CriticalFlaw", true);
        Directory.Delete($"{localPath}\\TF2HUD.Editor", true);
        Directory.Delete("JSON", true);
        MainWindow.UpdateAppSchema(true);
    }

    //private void TitleBar_MouseDown(object sender, MouseButtonEventArgs e)
    //{
    //    if (e.ChangedButton == MouseButton.Left) DragMove();
    //}

    private void CloseButton_Click(object sender, RoutedEventArgs e)
    {
        Close();
    }
}