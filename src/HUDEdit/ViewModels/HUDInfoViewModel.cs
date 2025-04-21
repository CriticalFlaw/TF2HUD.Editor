using CommunityToolkit.Mvvm.Input;
using HUDEdit.Classes;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace HUDEdit.ViewModels;

internal partial class HUDInfoViewModel : ViewModelBase
{
    private readonly MainWindowViewModel _mainWindowViewModel;
    public HUD Hud { get; }
    public string Name => Hud.Name;
    public string Author => Hud.Author;
    public string Description => Hud.Description;
    private ObservableCollection<ScreenshotViewModel> _screenshots;
    public IEnumerable<ScreenshotViewModel> Screenshots => _screenshots;
    public int Column { get; set; }
    public int Row { get; set; }

    public HUDInfoViewModel(MainWindowViewModel mainWindowViewModel, HUD hud)
    {
        _mainWindowViewModel = mainWindowViewModel;
        Hud = hud;
        _screenshots = new ObservableCollection<ScreenshotViewModel>();
        var screenshots = Hud.Screenshots ?? Array.Empty<object>();

        for (int i = 0; i < screenshots.Length; i++)
        {
            var imageUrl = screenshots[i]?.ToString();
            if (string.IsNullOrWhiteSpace(imageUrl))
                continue;

            var image = Utilities.LoadImage(imageUrl);

            _screenshots.Add(new ScreenshotViewModel
            {
                ImageSource = image,
                Row = i / 2,
                Column = i % 2
            });
        }
    }

    [RelayCommand]
    public void BtnCustomize_Click()
    {
        _mainWindowViewModel.SelectHUD(Hud);
    }
}
