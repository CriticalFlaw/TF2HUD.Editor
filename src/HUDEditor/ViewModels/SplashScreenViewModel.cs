using System.Threading;

namespace HUDEditor.ViewModels;

public partial class SplashScreenViewModel : ViewModelBase
{
    private string _startupMessage;
    public string StartupMessage
    {
        get => _startupMessage;
        set
        {
            _startupMessage = value;
            OnPropertyChanged(nameof(StartupMessage));
        }
    }

    public void Cancel()
    {
        StartupMessage = Assets.Resources.ui_splash_cancel;
        _cts.Cancel();
    }

    private readonly CancellationTokenSource _cts = new();

    public CancellationToken CancellationToken => _cts.Token;
}