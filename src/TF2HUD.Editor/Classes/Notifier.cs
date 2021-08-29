using log4net;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace HUDEditor.Classes
{
    public class Notifier
    {
        private readonly ILog _logger;

        public Notifier(ILog logger)
        {
            _logger = logger;
        }

        /// <summary>
        ///     Display a message box with a message to the user and log it.
        /// </summary>
        public MessageBoxResult ShowMessageBox(MessageBoxImage type, string message, MessageBoxButton buttons = MessageBoxButton.OK)
        {
            LogMessage(type, message);
            return MessageBox.Show(message, string.Empty, buttons, type);
        }

        private void LogMessage(MessageBoxImage type, string message)
        {
            switch (type)
            {
                case MessageBoxImage.Error:
                    _logger.Error(message);
                    break;
                case MessageBoxImage.Warning:
                    _logger.Warn(message);
                    break;
            }
        }
    }
}
