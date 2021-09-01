using System.Windows;

namespace HUDEditor.Classes
{
    public interface INotifier
    {
        MessageBoxResult ShowMessageBox(MessageBoxImage type, string message, MessageBoxButton buttons = MessageBoxButton.OK);
    }
}