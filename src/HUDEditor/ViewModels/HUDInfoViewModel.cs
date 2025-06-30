using Avalonia.Media.Imaging;
using CommunityToolkit.Mvvm.Input;
using HUDEditor.Classes;
using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace HUDEditor.ViewModels;

internal partial class HUDInfoViewModel : ViewModelBase
{
    private readonly MainWindowViewModel _mainWindowViewModel;
    public HUD Hud { get; }
    public string Name => Hud.Name;
    public string Author => string.Format(Assets.Resources.ui_author, Hud.Author);
    public string Description => Hud.Description;
    private ObservableCollection<ScreenshotViewModel> _screenshots;
    public IEnumerable<ScreenshotViewModel> Screenshots => _screenshots;
    public int Column { get; set; }
    public int Row { get; set; }

    public HUDInfoViewModel(MainWindowViewModel mainWindowViewModel, HUD hud)
    {
        _mainWindowViewModel = mainWindowViewModel;
        Hud = hud;
        _screenshots = [];

        // Use cached ScreenshotImages if available, otherwise fallback to empty
        var images = Hud.ScreenshotImages ?? new List<Bitmap>();

        for (int i = 0; i < images.Count; i++)
        {
            var image = images[i];
            if (image == null)
                continue;

            _screenshots.Add(new ScreenshotViewModel
            {
                ImageSource = image,
                Row = i / 2,
                Column = i % 2
            });
        }
    }

    [RelayCommand]
    public void CustomizeHUD() => _mainWindowViewModel.SelectHUD(Hud);
}
