using System;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Net;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Forms;
using AutoUpdaterDotNET;
using log4net;
using log4net.Config;
using Microsoft.Win32;
using TF2HUD.Editor.Common;
using TF2HUD.Editor.Properties;
using Application = System.Windows.Application;
using MessageBox = System.Windows.MessageBox;

namespace TF2HUD.Editor
{
    /// <summary>
    ///     Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow
    {
        public static string HudSelection = Settings.Default.hud_selected;
        public static string HudPath = Settings.Default.hud_directory;
        public static readonly ILog Logger = LogManager.GetLogger(MethodBase.GetCurrentMethod()?.DeclaringType);

        public MainWindow()
        {
            var repository = LogManager.GetRepository(Assembly.GetEntryAssembly());
            XmlConfigurator.Configure(repository, new FileInfo("log4net.config"));
            Logger.Info("INITIALIZING...");
            InitializeComponent();
            foreach (Enum item in Enum.GetValues(typeof(Utilities.HUDs)))
                lbSelectHud.Items.Add(Utilities.GetStringValue(item));
            SetupDirectory();
            SetupHUD();
            AutoUpdater.OpenDownloadPage = true;
            AutoUpdater.Start(Properties.Resources.app_update);
        }

        /// <summary>
        ///     Download the latest version of a given HUD.
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
        ///     Check if Team Fortress 2 is currently running.
        /// </summary>
        public static bool CheckGameStatus()
        {
            if (!Process.GetProcessesByName("hl2").Any()) return true;
            MessageBox.Show(Properties.Resources.info_game_running_desc,
                Properties.Resources.info_game_running, MessageBoxButton.OK, MessageBoxImage.Information);
            return false;
        }

        public static bool SetupHUD()
        {
            return true;
        }

        /// <summary>
        ///     Setup the tf/custom directory, if not already set.
        /// </summary>
        public void SetupDirectory(bool userSet = false)
        {
            if (!SearchRegistry() && !CheckUserPath() || userSet)
            {
                Logger.Info("Setting the tf/custom directory. Opening folder browser, asking the user.");
                using (var browser = new FolderBrowserDialog
                {
                    Description = Properties.Resources.info_folder_browser,
                    ShowNewFolderButton = true
                })
                {
                    while (!browser.SelectedPath.Contains("tf\\custom"))
                        if (browser.ShowDialog() == System.Windows.Forms.DialogResult.OK &&
                            browser.SelectedPath.Contains("tf\\custom"))
                        {
                            Settings.Default.hud_directory = browser.SelectedPath;
                            Settings.Default.Save();
                            HudPath = Settings.Default.hud_directory;
                            Logger.Info("Directory has been set to " + Settings.Default.hud_directory);
                        }
                        else
                        {
                            break;
                        }
                }

                if (!CheckUserPath())
                {
                    Logger.Error("Unable to set the tf/custom directory. Exiting.");
                    MessageBox.Show(Properties.Resources.error_app_directory,
                        Properties.Resources.error_app_directory_title, MessageBoxButton.OK,
                        MessageBoxImage.Information);
                    Application.Current.Shutdown();
                }
            }

            CleanDirectory();
            SetFormControls();
        }

        /// <summary>
        ///     Cleans up the tf/custom and installer directories
        /// </summary>
        private void CleanDirectory()
        {
            Logger.Info($"Cleaning-up {HudSelection} directories...");

            // Clean the application directory
            if (File.Exists(Directory.GetCurrentDirectory() + "\\temp.zip"))
                File.Delete(Directory.GetCurrentDirectory() + "\\temp.zip");

            // Clean the tf/custom directory
            var hudDirectory = Directory.Exists(HudPath + $"\\{HudSelection}-master")
                ? HudPath + $"\\{HudSelection}-master"
                : string.Empty;

            if (!string.IsNullOrEmpty(hudDirectory))
            {
                // Remove the previous backup if found.
                if (File.Exists(HudPath + $"\\{HudSelection}-backup.zip"))
                    File.Delete(HudPath + $"\\{HudSelection}-backup.zip");

                Logger.Info($"Found a {HudSelection} installation. Creating a back-up.");
                ZipFile.CreateFromDirectory(hudDirectory, HudPath + $"\\{HudSelection}-backup.zip");
                Directory.Delete(hudDirectory, true);
                MessageBox.Show(string.Format(Properties.Resources.info_create_backup, HudSelection),
                    Properties.Resources.info_create_backup_title,
                    MessageBoxButton.OK, MessageBoxImage.Information);
            }

            Logger.Info("Cleaning-up HUD directories...Done!");
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
        ///     Check if the HUD is installed in the tf/custom directory
        /// </summary>
        public bool CheckHudPath()
        {
            return Directory.Exists(HudPath + "\\" + HudSelection);
        }

        /// <summary>
        ///     Check if user's directory setting is valid
        /// </summary>
        public static bool CheckUserPath()
        {
            return !string.IsNullOrWhiteSpace(HudPath) && HudPath.Contains("tf\\custom");
        }

        /// <summary>
        ///     Update the installer controls like labels and buttons
        /// </summary>
        public void SetFormControls()
        {
            if (Directory.Exists(HudPath) && CheckUserPath())
            {
                var isInstalled = CheckHudPath();
                BtnInstall.IsEnabled = true;
                BtnInstall.Content = isInstalled ? "Reinstall" : "Install";
                BtnSave.IsEnabled = isInstalled;
                BtnUninstall.IsEnabled = isInstalled;
                LblStatus.Content = $"{HudSelection} is {(!isInstalled ? "not " : "")}installed...";
            }
            else
            {
                BtnInstall.IsEnabled = false;
                BtnInstall.Content = "Set Directory";
                BtnSave.IsEnabled = false;
                BtnUninstall.IsEnabled = false;
                LblStatus.Content = "Directory is not set...";
            }

            BtnInstall.IsEnabled = gbSelectHUD.Visibility != Visibility.Visible;
            BtnReset.IsEnabled = gbSelectHUD.Visibility != Visibility.Visible;
        }

        /// <summary>
        ///     Display the error message box
        /// </summary>
        public static void ShowErrorMessage(string message, string exception)
        {
            MessageBox.Show($@"{message} {exception}", string.Format(Properties.Resources.error_info, "Error"),
                MessageBoxButton.OK, MessageBoxImage.Error);
            Logger.Error(exception);
        }

        private void BtnInstall_OnClick(object sender, RoutedEventArgs e)
        {
            try
            {
                if (!CheckUserPath())
                {
                    Logger.Info("Opening Directory Browser...");
                    SetupDirectory(true);
                    return;
                }

                if (!CheckGameStatus()) return;

                var worker = new BackgroundWorker();
                worker.DoWork += (_, _) =>
                {
                    Dispatcher.Invoke(() =>
                    {
                        Logger.Info($"Installing {HudSelection}...");
                        Logger.Info($"Downloading the latest {HudSelection}...");
                        switch (HudSelection.ToLowerInvariant())
                        {
                            case "flawhud":
                                DownloadHud(Properties.Resources.download_flawhud);
                                break;

                            case "rayshud":
                                DownloadHud(Properties.Resources.download_rayshud);
                                break;
                        }

                        Logger.Info($"Downloading the latest {HudSelection}...Done!");
                        Logger.Info($"Extracting downloaded {HudSelection} to " + HudPath);
                        ZipFile.ExtractToDirectory(Directory.GetCurrentDirectory() + $"\\{HudSelection}.zip", HudPath);
                        if (Directory.Exists(HudPath + $"\\{HudSelection}"))
                            Directory.Delete(HudPath + $"\\{HudSelection}", true);
                        if (Directory.Exists(HudPath + $"\\{HudSelection}-master"))
                            Directory.Move(HudPath + $"\\{HudSelection}-master", HudPath + $"\\{HudSelection}");
                        Logger.Info($"Extracting downloaded {HudSelection}...Done!");
                        CleanDirectory();

                        switch (HudSelection.ToLowerInvariant())
                        {
                            case "flawhud":
                                uiFlawHud.SaveHudSettings();
                                uiFlawHud.ApplyHudSettings();
                                break;

                            case "rayshud":
                                uiRaysHud.SaveHudSettings();
                                uiRaysHud.ApplyHudSettings();
                                break;
                        }

                        SetFormControls();
                    });
                };
                worker.RunWorkerCompleted += (_, _) =>
                {
                    LblStatus.Content = "Installation finished at " + DateTime.Now;
                    Logger.Info($"Installing {HudSelection}...Done!");
                    MessageBox.Show(string.Format(Properties.Resources.info_install_complete_desc, HudSelection),
                        Properties.Resources.info_install_complete, MessageBoxButton.OK, MessageBoxImage.Information);
                };
                worker.RunWorkerAsync();
            }
            catch (Exception ex)
            {
                ShowErrorMessage(string.Format(Properties.Resources.error_app_install, HudSelection), ex.Message);
            }
        }

        private void BtnUninstall_OnClick(object sender, RoutedEventArgs e)
        {
            try
            {
                if (!CheckGameStatus()) return;
                Logger.Info($"Uninstalling {HudSelection}...");
                if (!CheckHudPath()) return;
                Directory.Delete(HudPath + $"\\{HudSelection}", true);
                LblStatus.Content = $"Uninstalled {HudSelection} at " + DateTime.Now;
                SetupDirectory();
                Logger.Info($"Uninstalling {HudSelection}...Done!");
                MessageBox.Show(string.Format(Properties.Resources.info_uninstall_complete_desc, HudSelection),
                    Properties.Resources.info_uninstall_complete, MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                ShowErrorMessage(string.Format(Properties.Resources.error_app_uninstall, HudSelection), ex.Message);
            }
        }

        private void BtnSave_OnClick(object sender, RoutedEventArgs e)
        {
            Logger.Info("Applying HUD Settings...");
            var worker = new BackgroundWorker();
            worker.DoWork += (_, _) =>
            {
                Dispatcher.Invoke(() =>
                {
                    switch (HudSelection.ToLowerInvariant())
                    {
                        case "flawhud":
                            uiFlawHud.SaveHudSettings();
                            uiFlawHud.ApplyHudSettings();
                            break;

                        case "rayshud":
                            uiRaysHud.SaveHudSettings();
                            uiRaysHud.ApplyHudSettings();
                            break;
                    }
                });
            };
            //worker.RunWorkerCompleted();
            worker.RunWorkerAsync();
            LblStatus.Content = "Settings Saved at " + DateTime.Now;
            Logger.Info("Resetting HUD Settings...Done!");
        }

        private void BtnReset_OnClick(object sender, RoutedEventArgs e)
        {
            switch (HudSelection.ToLowerInvariant())
            {
                case "flawhud":
                    uiFlawHud.ResetHudSettings();
                    break;

                case "rayshud":
                    uiRaysHud.ResetHudSettings();
                    break;
            }

            LblStatus.Content = "Settings Reset at " + DateTime.Now;
        }

        private void BtnReportIssue_OnClick(object sender, RoutedEventArgs e)
        {
            try
            {
                Logger.Info("Opening Issue Tracker...");
                Process.Start(Properties.Resources.app_tracker);
            }
            catch
            {
                var url = Properties.Resources.app_tracker;
                if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
                {
                    url = url.Replace("&", "^&");
                    Process.Start(new ProcessStartInfo("cmd", $"/c start {url}") {CreateNoWindow = true});
                }
                else if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
                {
                    Process.Start("xdg-open", url);
                }
                else if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
                {
                    Process.Start("open", url);
                }
                else
                {
                    throw;
                }
            }
        }

        private void BtnSwitch_OnClick(object sender, RoutedEventArgs e)
        {
            gbSelectHUD.Visibility = Visibility.Visible;
            uiFlawHud.Visibility = Visibility.Hidden;
            uiRaysHud.Visibility = Visibility.Hidden;
            SetFormControls();
        }

        private void lbSelectHud_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            switch (lbSelectHud.SelectedValue.ToString().ToLowerInvariant())
            {
                case "flawhud":
                    gbSelectHUD.Visibility = Visibility.Hidden;
                    uiFlawHud.Visibility = Visibility.Visible;
                    uiRaysHud.Visibility = Visibility.Hidden;
                    break;

                case "rayshud":
                    gbSelectHUD.Visibility = Visibility.Hidden;
                    uiFlawHud.Visibility = Visibility.Hidden;
                    uiRaysHud.Visibility = Visibility.Visible;
                    break;

                default:
                    gbSelectHUD.Visibility = Visibility.Visible;
                    uiFlawHud.Visibility = Visibility.Hidden;
                    uiRaysHud.Visibility = Visibility.Hidden;
                    break;
            }

            Settings.Default.hud_selected = lbSelectHud.SelectedValue.ToString();
            Settings.Default.Save();
            HudSelection = Settings.Default.hud_selected.ToLowerInvariant();

            SetFormControls();
        }
    }
}