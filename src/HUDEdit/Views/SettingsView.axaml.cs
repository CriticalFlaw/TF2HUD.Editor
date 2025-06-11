using HUDEdit.ViewModels;

namespace HUDEdit.Views;

public partial class SettingsView : Avalonia.Controls.Window
{
    public SettingsView()
    {
        InitializeComponent();
        DataContext = new SettingsViewModel();

        //// Set the flag so the event handler knows we're still initializing
        //_isInitializing = true;

        //// Set the radio button based on selected user language.
        //switch (App.Config.ConfigSettings.UserPrefs.Language)
        //{
        //    case "fr-FR":
        //        BtnLocalizeFr.IsChecked = true;
        //        break;
        //    case "ru-RU":
        //        BtnLocalizeRu.IsChecked = true;
        //        break;
        //    case "pt-BR":
        //        BtnLocalizeBr.IsChecked = true;
        //        break;
        //    case "it":
        //        BtnLocalizeIt.IsChecked = true;
        //        break;
        //    case "zh-CN":
        //        BtnLocalizeCn.IsChecked = true;
        //        break;
        //    default:
        //        BtnLocalizeEn.IsChecked = true;
        //        break;
        //}

        //_isInitializing = false;
    }

    /// <summary>
    /// Updates localization to the selected language.
    /// </summary>
    private void BtnLocalize_Click(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
    {
        //// Ignore clicks during initialization
        //if (_isInitializing) return;

        //if (BtnLocalizeEn.IsChecked == true)
        //    Assets.Resources.Culture = new CultureInfo("en-US");
        //else if (BtnLocalizeFr.IsChecked == true)
        //    Assets.Resources.Culture = new CultureInfo("fr-FR");
        //else if (BtnLocalizeRu.IsChecked == true)
        //    Assets.Resources.Culture = new CultureInfo("ru-RU");
        //else if (BtnLocalizeBr.IsChecked == true)
        //    Assets.Resources.Culture = new CultureInfo("pt-BR");
        //else if (BtnLocalizeIt.IsChecked == true)
        //    Assets.Resources.Culture = new CultureInfo("it");
        //else if (BtnLocalizeCn.IsChecked == true)
        //    Assets.Resources.Culture = new CultureInfo("zh-CN");

        //// Save language preference then restart.
        //App.Config.ConfigSettings.UserPrefs.Language = Assets.Resources.Culture.ToString();
        //App.SaveConfiguration();
        //Utilities.RestartApplication();
    }
}