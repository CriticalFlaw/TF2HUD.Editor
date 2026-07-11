using System.Reflection;

namespace HUDEditor.ViewModels;

public partial class AppInfoViewModel : ViewModelBase
{
    public string AppVersion => Assembly.GetExecutingAssembly().GetName().Version?.ToString(2) ?? "4.4";
}