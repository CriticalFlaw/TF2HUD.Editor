using HUDEditor.ViewModels;

namespace HUDEditor.Views;

public partial class SettingsView : Avalonia.Controls.Window
{
    public SettingsView()
    {
        InitializeComponent();
        DataContext = new SettingsViewModel();
    }
}