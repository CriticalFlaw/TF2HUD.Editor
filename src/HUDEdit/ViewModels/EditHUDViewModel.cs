using Avalonia.Controls;
using CommunityToolkit.Mvvm.Input;
using HUDEdit.Classes;
using HUDEdit.Views;
using HUDEdit.Models;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Linq;

namespace HUDEdit.ViewModels;

internal partial class EditHUDViewModel : ViewModelBase
{
    private readonly MainWindowViewModel _mainWindowViewModel;
    private readonly HUD _hud;
    private string _status;
    public string Status
    {
        get => _status;
        set
        {
            _status = value;
            OnPropertyChanged(nameof(Status));
        }
    }
    public IEnumerable<PresetViewModel> Presets { get; }
    private Preset _selectedPreset;
    public Preset SelectedPreset
    {
        get => _selectedPreset;
        set
        {
            _selectedPreset = value;
            OnPropertyChanged(nameof(SelectedPreset));
        }
    }
    public string DownloadLink => _hud.DownloadUrl;
    public string GitHubUrl => _hud.GitHubUrl;
    public string TF2HudsUrl => _hud.TF2HudsUrl;
    public string ComfigHudsUrl => _hud.ComfigHudsUrl;
    public string DiscordUrl => _hud.DiscordUrl;
    public string SteamUrl => _hud.SteamUrl;
    public Grid Content => _hud.GetControls();

    public EditHUDViewModel(MainWindowViewModel mainWindowViewModel, HUD hud)
    {
        _mainWindowViewModel = mainWindowViewModel;
        _hud = hud;

        if (Utilities.CheckHudInstallation(hud))
            _status = string.Format(Assets.Resources.status_installed, hud.Name);
        else
            _status = string.Format(Assets.Resources.status_installed_not, hud.Name);

        if ((App.Config.ConfigSettings.UserPrefs.PathBypass && !Utilities.CheckUserPath()) ||
            (!App.Config.ConfigSettings.UserPrefs.PathBypass & !MainWindow.HudPath.EndsWith("tf/custom")))
            _status = Assets.Resources.status_path_notset;

        _selectedPreset = _hud.Settings.Preset;
        Presets = new ObservableCollection<PresetViewModel>(Enum.GetValues<Preset>().Select((p) => new PresetViewModel(this, p)));
        _mainWindowViewModel.WindowTitle = $"{Assets.Resources.ui_title} | {hud.Name}";
    }

    private void MainWindowViewModelPropertyChanged(object sender, PropertyChangedEventArgs e)
    {
        if (e.PropertyName == nameof(MainWindowViewModel.SelectedHudInstalled))
        {
            OnPropertyChanged(nameof(Status));
        }
    }

    [RelayCommand]
    public void ChangePreset(Preset preset)
    {
        _hud.SetPreset(preset);

        foreach (PresetViewModel p in Presets)
        {
            p.Selected = p.Preset == _hud.Settings.Preset;
        }
        
        OnPropertyChanged(nameof(Content));
    }

    [RelayCommand]
    public void OpenWebpage(string url)
    {
        Utilities.OpenWebpage(url);
    }

    public override void Dispose()
    {
        base.Dispose();
        _mainWindowViewModel.PropertyChanged -= MainWindowViewModelPropertyChanged;
        foreach (PresetViewModel p in Presets)
        {
            p.Dispose();
        }
    }
}