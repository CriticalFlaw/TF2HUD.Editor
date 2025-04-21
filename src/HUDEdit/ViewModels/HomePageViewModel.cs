using CommunityToolkit.Mvvm.Input;
using HUDEdit.Classes;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Windows.Data;

namespace HUDEdit.ViewModels;

internal partial class HomePageViewModel : ViewModelBase
{
    private readonly MainWindowViewModel _mainWindowViewModel;
    private ObservableCollection<HUDButtonViewModel> _hudList;
    public ObservableCollection<HUDButtonViewModel> HUDListView { get; } = new();
    private string _searchText = "";
    public string SearchText
    {
        get => _searchText;
        set
        {
            _searchText = value.ToLower();
            OnPropertyChanged(nameof(SearchText));
            //HUDListView.Refresh();
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
            //HUDListView.Refresh();
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

        // Generate HUD buttons and set positions
        var hudButtonList = hudList
            .Select((hud, i) =>
                new HUDButtonViewModel(hud, i % 2, i / 2))
            .OrderBy(x => x.Name)
            .ToList();

        _hudList = new ObservableCollection<HUDButtonViewModel>(
            hudList.Select((hud, i) =>
                new HUDButtonViewModel(hud, i % 2, i / 2)) // Column = i % 2, Row = i / 2
            .OrderBy(x => x.Name)
        );

        // Setup filtering
        HUDListView.Clear();

        foreach (var item in _hudList)
            HUDListView.Add(item);

        _info = new AppInfoViewModel();
        _mainWindowViewModel.PropertyChanged += MainWindowViewModelPropertyChanged;
    }

    private bool Filter(object o)
    {
        HUDButtonViewModel hud = (HUDButtonViewModel)o;
        return (!DisplayUniqueHudsOnly || hud.Unique) && (hud.Name.ToLower().Contains(SearchText) || hud.Author.ToLower().Contains(SearchText));
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
    public void BtnDisplayUniqueHudsOnly_Click()
    {
    	DisplayUniqueHudsOnly = !DisplayUniqueHudsOnly;
    }
    
    public override void Dispose()
    {
        base.Dispose();
        _mainWindowViewModel.PropertyChanged -= MainWindowViewModelPropertyChanged;
    }
}