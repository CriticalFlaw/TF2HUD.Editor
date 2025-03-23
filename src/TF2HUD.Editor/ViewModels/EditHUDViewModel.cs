using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Windows.Controls;
using CommunityToolkit.Mvvm.Input;
using HUDEditor.Classes;
using HUDEditor.Models;

namespace HUDEditor.ViewModels
{
    public partial class EditHUDViewModel : ViewModelBase
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

        public IEnumerable<Download> Downloads { get; }
        private Download _selectedDownloadSource;

        public Download SelectedDownloadSource
        {
            get => _selectedDownloadSource;
            set
            {
                _selectedDownloadSource = value;
                OnPropertyChanged(nameof(SelectedDownloadSource));
            }
        }

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

            if (MainWindow.CheckHudInstallation(hud))
                _status = string.Format(Shared.Resources.status_installed, hud.Name);
            else if (Directory.Exists(MainWindow.HudPath))
                _status = string.Format(Shared.Resources.status_installed_not, hud.Name);
            else
                _status = Shared.Resources.status_pathNotSet;

            _selectedPreset = _hud.Settings.Preset;
            Presets = new ObservableCollection<PresetViewModel>(Enum.GetValues<Preset>().Select((p) => new PresetViewModel(this, p)));
            Downloads = new ObservableCollection<Download>(_hud.Download);
            _selectedDownloadSource = _hud.Download.First();
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
}