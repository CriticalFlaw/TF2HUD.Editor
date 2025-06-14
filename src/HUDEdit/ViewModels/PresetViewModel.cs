using HUDEdit.Models;
using System.ComponentModel;

namespace HUDEdit.ViewModels;

internal class PresetViewModel : ViewModelBase
{
    private readonly EditHUDViewModel _editHudViewModel;
    public Preset Preset { get; }
    private bool _selected;
    public bool Selected
    {
        get => _selected;
        set
        {
            _selected = value;
            OnPropertyChanged(nameof(Selected));
        }
    }

    public PresetViewModel(EditHUDViewModel editHudViewModel, Preset preset)
    {
        _editHudViewModel = editHudViewModel;
        Preset = preset;
        _selected = editHudViewModel.SelectedPreset == preset;
        _editHudViewModel.PropertyChanged += EditHUDViewModelPropertyChanged;
    }

    private void EditHUDViewModelPropertyChanged(object sender, PropertyChangedEventArgs e)
    {
        if (e.PropertyName == nameof(EditHUDViewModel.SelectedPreset))
            Selected = _editHudViewModel.SelectedPreset == Preset;
    }

    public override void Dispose()
    {
        _editHudViewModel.PropertyChanged -= EditHUDViewModelPropertyChanged;
    }
}