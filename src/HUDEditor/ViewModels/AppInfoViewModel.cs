using System.Reflection;

namespace HUDEditor.ViewModels;

internal partial class AppInfoViewModel : ViewModelBase
{
    public string AppVersion => Assembly.GetExecutingAssembly().GetName().Version?.ToString(2) ?? "4.4";
}