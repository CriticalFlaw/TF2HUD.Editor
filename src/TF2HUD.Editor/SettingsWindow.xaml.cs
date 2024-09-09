using HUDEditor.Properties;
using System;
using System.Globalization;
using System.IO;
using System.Windows;
using System.Windows.Input;
using WPFLocalizeExtension.Engine;

namespace HUDEditor
{
    /// <summary>
    /// Interaction logic for Settings.xaml
    /// </summary>
    public partial class SettingsWindow : Window
    {
        public SettingsWindow()
        {
            InitializeComponent();

            // Check for user selected settings.
            BtnPersistXhair.IsChecked = Settings.Default.app_xhair_persist;
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
            else if (BtnLocalizeChs.IsChecked == true)
                LocalizeDictionary.Instance.Culture = new CultureInfo("zh-CN");

            // Save language preference to user settings.
            Settings.Default.user_language = LocalizeDictionary.Instance.Culture.ToString();
            Settings.Default.Save();
        }

        private void BtnSetDirectory_OnClick(object sender, RoutedEventArgs e)
        {
            MainWindow.Logger.Info("Attempting to change the target directory.");
            MainWindow.SetupDirectory(true);
        }

        private void BtnRefresh_OnClick(object sender, RoutedEventArgs e)
        {
            MainWindow.UpdateAppSchema(false);
        }

        private void BtnPersistXhair_Click(object sender, RoutedEventArgs e)
        {
            Settings.Default.app_xhair_persist = BtnPersistXhair.IsChecked ?? true;
            Settings.Default.Save();
        }

        private void btnClearCache_Click(object sender, RoutedEventArgs e)
        {
            if (MainWindow.ShowMessageBox(MessageBoxImage.Information, Properties.Resources.info_clear_cache, MessageBoxButton.YesNo) != MessageBoxResult.Yes) return;

            var localPath = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
            Directory.Delete($"{localPath}\\CriticalFlaw", true);
            Directory.Delete($"{localPath}\\TF2HUD.Editor", true);
            Directory.Delete("JSON", true);
            MainWindow.UpdateAppSchema(true);
        }

        private void TitleBar_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ChangedButton == MouseButton.Left) DragMove();
        }

        private void CloseButton_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }
    }
}