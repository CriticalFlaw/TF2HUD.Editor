using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Reflection;
using System.Windows;
using System.Windows.Controls;
using AutoUpdaterDotNET;
using log4net;
using log4net.Config;
using Microsoft.Win32;
using TF2HUD.Editor.Properties;

namespace TF2HUD.Editor
{
    /// <summary>
    ///     Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public static readonly ILog Logger = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        public MainWindow()
        {
            var repository = LogManager.GetRepository(Assembly.GetEntryAssembly());
            XmlConfigurator.Configure(repository, new FileInfo("log4net.config"));
            Logger.Info("INITIALIZING...");
            InitializeComponent();
            AutoUpdater.OpenDownloadPage = true;
            AutoUpdater.Start(Properties.Resources.app_update);
        }

        /// <summary>
        ///     Calls to download the latest version of FlawHUD
        /// </summary>
        public static void DownloadHud(string download)
        {
            ServicePointManager.Expect100Continue = true;
            ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;
            ServicePointManager.ServerCertificateValidationCallback = delegate { return true; };
            var client = new WebClient();
            client.DownloadFile(download, "temp.zip");
            client.Dispose();
        }

        /// <summary>
        ///     Check if Team Fortress 2 is currently running
        /// </summary>
        public static bool CheckGameStatus()
        {
            if (!Process.GetProcessesByName("hl2").Any()) return true;
            MessageBox.Show(Properties.Resources.info_game_running_desc,
                Properties.Resources.info_game_running, MessageBoxButton.OK, MessageBoxImage.Information);
            return false;
        }

        public static bool SearchRegistry()
        {
            Logger.Info("Looking for the Team Fortress 2 directory...");
            var is64Bit = Environment.Is64BitProcess ? "Wow6432Node\\" : string.Empty;
            var registry = (string) Registry.GetValue($@"HKEY_LOCAL_MACHINE\Software\{is64Bit}Valve\Steam",
                "InstallPath", null);
            if (!string.IsNullOrWhiteSpace(registry))
            {
                registry += "\\steamapps\\common\\Team Fortress 2\\tf\\custom";
                if (Directory.Exists(registry))
                {
                    Settings.Default.Save();
                    Logger.Info("Directory found at " + registry);
                    return true;
                }
            }

            Logger.Info("Directory not found. Continuing...");
            return false;
        }

        /// <summary>
        ///     Display the error message box
        /// </summary>
        public static void ShowErrorMessage(string title, string message, string exception)
        {
            MessageBox.Show($@"{message} {exception}", string.Format(Properties.Resources.error_info, title),
                MessageBoxButton.OK, MessageBoxImage.Error);
            Logger.Error(exception);
        }

        private void SelectHud_OnSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (SelectHud.SelectedIndex == 0)
            {
                flawhud.Visibility = Visibility.Visible;
                rayshud.Visibility = Visibility.Hidden;
            }
            else
            {
                flawhud.Visibility = Visibility.Hidden;
                rayshud.Visibility = Visibility.Visible;
            }
        }
    }
}