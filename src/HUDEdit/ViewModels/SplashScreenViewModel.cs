using System.Threading;

namespace HUDEdit.ViewModels;

public partial class SplashScreenViewModel : ViewModelBase
{
    private string _startupMessage = "Starting up... Please wait.";
    public string StartupMessage
    {
        get => _startupMessage;
        set => SetProperty(ref _startupMessage, value);
    }

    public void Cancel()
    {
        StartupMessage = "Cancelling...";
        _cts.Cancel();
    }

    private readonly CancellationTokenSource _cts = new();

    public CancellationToken CancellationToken => _cts.Token;
}