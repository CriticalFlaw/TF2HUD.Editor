using HUDEdit.ViewModels;

namespace HUDEdit.Views;

public partial class SettingsView : Avalonia.Controls.Window
{
    public SettingsView()
    {
        InitializeComponent();
        DataContext = new SettingsViewModel();
    }
}