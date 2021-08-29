using HUDEditor.Properties;
using log4net;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Forms;

namespace HUDEditor.Classes
{
    public class HudDirectory
    {
        private readonly ILog _logger;
        private readonly Notifier _notifier;

        public HudDirectory(ILog logger, Notifier notifier)
        {
            _logger = logger;
            _notifier = notifier;
        }

        /// <summary>
        ///     Setup the tf/custom directory, if it's not already set.
        /// </summary>
        /// <param name="userInitiated">Flags the process as being user initiated, skip right to the folder browser.</param>
        public void Setup(string currentHudPath, bool userInitiated = false)
        {
            if (!userInitiated && AlreadySetup(currentHudPath))
            {
                return;
            }

            SetupWithFolderBrowser();

            if (NotSetup(Settings.Default.hud_directory))
            {
                QuitApplication();
            }
        }
        
        private bool AlreadySetup(string currentHudPath)
        {
            return Utilities.CheckUserPath(currentHudPath) || Utilities.SearchRegistry();
        }

        private void SetupWithFolderBrowser()
        {
            _logger.Info("tf/custom directory is not set. Asking the user...");

            using var browser = new FolderBrowserDialog
            {
                Description = Properties.Resources.info_path_browser,
                UseDescriptionForTitle = true,
                ShowNewFolderButton = true
            };
            var selectedDirectory = GetSelectedDirectory(browser);
            if (selectedDirectory != null)
            {
                SaveDirectory(selectedDirectory);
            }
        }

        /// <summary>
        /// Loop until the user provides a valid tf/custom directory, unless they cancel out.
        /// </summary>
        /// <param name="browser"></param>
        /// <returns>The selected directory.</returns>
        private string GetSelectedDirectory(FolderBrowserDialog browser)
        {
            while (!IsValidDirectory(browser.SelectedPath))
            {
                if (browser.ShowDialog() != DialogResult.OK)
                {
                    return null;
                }

                if (!IsValidDirectory(browser.SelectedPath))
                {
                    _notifier.ShowMessageBox(MessageBoxImage.Error, Properties.Resources.info_path_invalid);
                    return null;
                }

                return browser.SelectedPath;
            }

            return null;
        }

        private static bool IsValidDirectory(string directory)
        {
            return directory?.EndsWith("tf\\custom") ?? false;
        }

        private void SaveDirectory(string selectedDirectory)
        {
            Settings.Default.hud_directory = selectedDirectory;
            Settings.Default.Save();
            _logger.Info($"tf/custom directory is set to: {Settings.Default.hud_directory}");
        }

        private static bool NotSetup(string currentHudPath)
        {
            return !Utilities.CheckUserPath(currentHudPath);
        }

        private void QuitApplication()
        {
            _logger.Info("tf/custom directory still not set. Exiting...");
            _notifier.ShowMessageBox(MessageBoxImage.Warning, Utilities.GetLocalizedString(Properties.Resources.error_app_directory));
            System.Windows.Application.Current.Shutdown();
        }
    }
}
