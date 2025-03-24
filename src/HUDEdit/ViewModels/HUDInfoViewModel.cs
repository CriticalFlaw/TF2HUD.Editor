namespace HUDEdit.ViewModels;

internal class HUDInfoViewModel : ViewModelBase
{
/*    private readonly MainWindowViewModel _mainWindowViewModel;
    public HUD Hud { get; }
    public string Name => Hud.Name;
    public string Author => Hud.Author;
    public string Description => Hud.Description;

    private ObservableCollection<object> _screenshots;
    public IEnumerable<object> Screenshots => _screenshots;

    public HUDInfoViewModel(MainWindowViewModel mainWindowViewModel, HUD hud)
    {
        _mainWindowViewModel = mainWindowViewModel;
        Hud = hud;
        _screenshots = new ObservableCollection<object>((Hud.Screenshots ?? Array.Empty<object>()).Select((screenshot, i) => new { ImageSource = screenshot, Column = i % 2, Row = i / 2 }));
    }

    [RelayCommand]
    public void BtnCustomize_Click()
    {
        _mainWindowViewModel.SelectHUD(Hud);
    }*/
}
