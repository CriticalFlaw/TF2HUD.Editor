using CommunityToolkit.Mvvm.Input;
using HUDEdit.Classes;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;

namespace HUDEdit.ViewModels;

internal partial class HomePageViewModel : ViewModelBase
{
    private readonly MainWindowViewModel _mainWindowViewModel;
    private List<HUDButtonViewModel> _allHuds;
    public ObservableCollection<HUDButtonViewModel> HUDListView { get; } = new();

    private string _searchText = "";
    public string SearchText
    {
        get => _searchText;
        set
        {
            _searchText = value.ToLowerInvariant();
            OnPropertyChanged(nameof(SearchText));
            ApplyFilters();
        }
    }

    private bool _displayUniqueHudsOnly;
    public bool DisplayUniqueHudsOnly
    {
        get => _displayUniqueHudsOnly;
        set
        {
            _displayUniqueHudsOnly = value;
            OnPropertyChanged(nameof(DisplayUniqueHudsOnly));
            ApplyFilters();
        }
    }

    private ViewModelBase _info;
    public ViewModelBase Info
    {
        get => _info;
        private set
        {
            _info = value;
            OnPropertyChanged(nameof(Info));
        }
    }

    public int Column { get; set; }
    public int Row { get; set; }

    public HomePageViewModel(MainWindowViewModel mainWindowViewModel, IEnumerable<HUD> hudList)
    {
        _mainWindowViewModel = mainWindowViewModel;

        // Convert to HUDButtonViewModel with column/row positioning
        _allHuds = hudList.Select((hud, i) => new HUDButtonViewModel(hud, i % 2, i / 2)).OrderBy(x => x.Name).ToList();
        ApplyFilters();

        Info = new AppInfoViewModel();
        _mainWindowViewModel.PropertyChanged += MainWindowViewModelPropertyChanged;
    }

    private void ApplyFilters()
    {
        HUDListView.Clear();

        var filtered = _allHuds.Where(x => (!DisplayUniqueHudsOnly || x.Unique) && (string.IsNullOrWhiteSpace(SearchText) || x.Name.ToLowerInvariant().Contains(SearchText)));
        foreach (var hud in filtered) HUDListView.Add(hud);
    }

    private void MainWindowViewModelPropertyChanged(object sender, PropertyChangedEventArgs e)
    {
        if (e.PropertyName == nameof(MainWindowViewModel.HighlightedHud))
        {
            Info?.Dispose();

            if (_mainWindowViewModel.HighlightedHud is not null)
            {
                Info = new HUDInfoViewModel(_mainWindowViewModel, _mainWindowViewModel.HighlightedHud);
            }
        }
    }

    [RelayCommand]
    public void DisplayUniqueHuds() => DisplayUniqueHudsOnly = !DisplayUniqueHudsOnly;

    public override void Dispose()
    {
        base.Dispose();
        _mainWindowViewModel.PropertyChanged -= MainWindowViewModelPropertyChanged;
    }
}