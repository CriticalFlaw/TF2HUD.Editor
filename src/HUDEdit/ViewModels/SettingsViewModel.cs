using HUDEdit.Classes;
using System.IO;
using System.Windows;
using System;
using Avalonia.Media.Imaging;
using AsyncImageLoader;
using System.Net.Http;
using System.Threading.Tasks;
using System.ComponentModel;

namespace HUDEdit.ViewModels;

internal partial class SettingsViewModel : ViewModelBase
{
    public static void BtnSetDirectory_OnClick()
    {
        App.Logger.Info("Attempting to change the target directory.");
        MainWindow.SetupDirectoryAsync(true);
    }

    public static void BtnRefresh_OnClick() => MainWindow.UpdateAppSchema(false);

    public static void BtnClearCache_Click()
    {
        if (Utilities.ShowMessageBox(MessageBoxImage.Information, Assets.Resources.info_clear_cache, MessageBoxButton.YesNo) != MessageBoxResult.Yes) return;

        var localPath = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
        Directory.Delete($"{localPath}\\CriticalFlaw", true);
        Directory.Delete($"{localPath}\\TF2HUD.Editor", true);
        Directory.Delete("JSON", true);
        MainWindow.UpdateAppSchema(true);
    }

    public event PropertyChangedEventHandler? PropertyChanged;
    protected virtual void OnPropertyChanged(string propertyName) => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
}