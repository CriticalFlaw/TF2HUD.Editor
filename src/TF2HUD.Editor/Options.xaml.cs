using HUDEditor.Properties;
using System.Globalization;
using System.Windows;
using WPFLocalizeExtension.Engine;

namespace HUDEditor
{
    /// <summary>
    /// Interaction logic for Settings.xaml
    /// </summary>
    public partial class Options : Window
    {
        public Options()
        {
            InitializeComponent();

            // Check for user selected settings.
            BtnAutoUpdate.IsChecked = Settings.Default.app_update_auto;
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
            MainWindow.Logger.Info("Attempting to change the 'tf/custom' directory.");
            MainWindow.SetupDirectory(true);
        }

        private void BtnRefresh_OnClick(object sender, RoutedEventArgs e)
        {
            MainWindow.CheckSchemaUpdates(false);
        }

        private void BtnAutoUpdate_OnClick(object sender, RoutedEventArgs e)
        {
            Settings.Default.app_update_auto = BtnAutoUpdate.IsChecked ?? true;
            Settings.Default.Save();
        }

        private void BtnPersistXhair_Click(object sender, RoutedEventArgs e)
        {
            Settings.Default.app_xhair_persist = BtnPersistXhair.IsChecked ?? true;
            Settings.Default.Save();
        }
    }
}