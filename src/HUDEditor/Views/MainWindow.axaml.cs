using HUDEditor.Classes;
using HUDEditor.ViewModels;
using System.ComponentModel;

namespace HUDEditor.Views;

public partial class MainWindow : Avalonia.Controls.Window
{
    public MainWindow()
    {
        InitializeComponent();

        // Run once the window is shown, so a folder picker (if needed) has a visible owner and errors aren't lost.
        Opened += async (_, _) =>
        {
            try
            {
                await Utilities.SetupDirectoryAsync(this);
#if !DEBUG
                // Check for updates
                if (App.Config.ConfigSettings.UserPrefs.AutoUpdate) await Utilities.UpdateAppSchema(true);
#endif
            }
            catch (System.Exception e)
            {
                App.Logger.Error($"Startup checks failed: {e}");
            }
        };
    }

    public void MainWindowViewModelPropertyChanged(object sender, PropertyChangedEventArgs e)
    {
        if (e.PropertyName == nameof(MainWindowViewModel.SelectedHud))
        {
            App.Config.ConfigSettings.UserPrefs.SelectedHUD = ((MainWindowViewModel)sender).SelectedHud?.Name ?? string.Empty;
        }
    }
}