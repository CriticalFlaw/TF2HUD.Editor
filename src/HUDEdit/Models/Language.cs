namespace HUDEdit.Models;

using System.ComponentModel;
using System.Runtime.CompilerServices;

public class Language : INotifyPropertyChanged
{
    public event PropertyChangedEventHandler? PropertyChanged;

    private string _cultureCode = string.Empty;
    public string CultureCode
    {
        get => _cultureCode;
        set
        {
            _cultureCode = value;
            OnPropertyChanged();
        }
    }

    private Avalonia.Media.Imaging.Bitmap _flagImagePath;
    public Avalonia.Media.Imaging.Bitmap FlagImagePath
    {
        get => _flagImagePath;
        set
        {
            _flagImagePath = value;
            OnPropertyChanged();
        }
    }

    protected void OnPropertyChanged([CallerMemberName] string? name = null) => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
}