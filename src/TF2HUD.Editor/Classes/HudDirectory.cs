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
        private readonly IUtilities _utilities;
        private readonly INotifier _notifier;
        private readonly IAppSettings _settings;
        private readonly IApplication _application;
        private readonly ILocalization _localization;
        private readonly IServiceProvider _serviceProvider;

        public HudDirectory(
            ILog logger,
            IUtilities utilities,
            INotifier notifier,
            IAppSettings settings,
            IApplication application,
            ILocalization localization,
            IServiceProvider serviceProvider)
        {
            _logger = logger;
            _utilities = utilities;
            _notifier = notifier;
            _settings = settings;
            _application = application;
            _localization = localization;
            _serviceProvider = serviceProvider;
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

            if (NotSetup(_settings.HudDirectory))
            {
                QuitApplication();
            }
        }

        private bool AlreadySetup(string currentHudPath)
        {
            return _utilities.CheckUserPath(currentHudPath) || _utilities.SearchRegistry();
        }

        private void SetupWithFolderBrowser()
        {
            _logger.Info("tf/custom directory is not set. Asking the user...");

            var browser = GetFolderBrowser();

            var selectedDirectory = GetSelectedDirectory(browser);
            if (selectedDirectory != null)
            {
                SaveDirectory(selectedDirectory);
            }
        }

        private IFolderBrowserDialog GetFolderBrowser()
        {
            IFolderBrowserDialog browser = (IFolderBrowserDialog)_serviceProvider.GetService(typeof(IFolderBrowserDialog));
            browser.Description = _localization.InfoPathBrowser;
            browser.UseDescriptionForTitle = true;
            browser.ShowNewFolderButton = true;
            return browser;
        }

        /// <summary>
        /// Loop until the user provides a valid tf/custom directory, unless they cancel out.
        /// </summary>
        /// <param name="browser"></param>
        /// <returns>The selected directory.</returns>
        private string GetSelectedDirectory(IFolderBrowserDialog browser)
        {
            var retryCount = 0;
            while (!IsValidDirectory(browser.SelectedPath))
            {
                if (browser.ShowDialog() != DialogResult.OK)
                    return null;

                if (!IsValidDirectory(browser.SelectedPath))
                    _notifier.ShowMessageBox(MessageBoxImage.Error, _localization.InfoPathInvalid);
                else
                    return browser.SelectedPath;

                retryCount++;

                if (retryCount >= 5)
                    return null;
            }

            return browser.SelectedPath;
        }

        private static bool IsValidDirectory(string directory)
        {
            return directory?.EndsWith("tf\\custom") ?? false;
        }

        private void SaveDirectory(string selectedDirectory)
        {
            _settings.HudDirectory = selectedDirectory;
            _settings.Save();
            _logger.Info($"tf/custom directory is set to: {_settings.HudDirectory}");
        }

        private bool NotSetup(string currentHudPath)
        {
            return !_utilities.CheckUserPath(currentHudPath);
        }

        private void QuitApplication()
        {
            _logger.Info("tf/custom directory still not set. Exiting...");
            _notifier.ShowMessageBox(MessageBoxImage.Warning, _utilities.GetLocalizedString(_localization.ErrorAppDirectory));
            _application.Shutdown();
        }
    }
}
