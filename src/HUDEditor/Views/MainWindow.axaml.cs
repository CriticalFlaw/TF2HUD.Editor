using HUDEditor.Classes;
using HUDEditor.ViewModels;
using System.ComponentModel;

namespace HUDEditor.Views;

public partial class MainWindow : Avalonia.Controls.Window
{
    public MainWindow()
    {
        InitializeComponent();
        Utilities.SetupDirectoryAsync(this);

#if !DEBUG
        // Check for updates
        if (App.Config.ConfigSettings.UserPrefs.AutoUpdate == true) Utilities.UpdateAppSchema(true);
#endif
    }

    public void MainWindowViewModelPropertyChanged(object sender, PropertyChangedEventArgs e)
    {
        if (e.PropertyName == nameof(MainWindowViewModel.SelectedHud))
        {
            App.Config.ConfigSettings.UserPrefs.SelectedHUD = ((MainWindowViewModel)sender).SelectedHud?.Name ?? string.Empty;
        }
    }
}