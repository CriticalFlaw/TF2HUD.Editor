using System.Windows;

namespace HUDEditor
{
    public class WindowsMessageBox : IMessageBox
    {
        public MessageBoxResult Show(string messageBoxText, string caption, MessageBoxButton button, MessageBoxImage icon)
        {
            return MessageBox.Show(messageBoxText, caption, button, icon);
        }
    }
}
