using System;
using System.Diagnostics;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Net;
using System.Reflection;
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
        public static string HudSelection = Settings.Default.hud_selected; // TODO: Should use the Enum type
        public static string HudPath = Settings.Default.hud_directory;
        public static readonly ILog Logger = LogManager.GetLogger(MethodBase.GetCurrentMethod()?.DeclaringType);
        public Json JSON;

        public MainWindow()
        {
            // Initialize the logger.
            var repository = LogManager.GetRepository(Assembly.GetEntryAssembly());
            XmlConfigurator.Configure(repository, new FileInfo("log4net.config"));
            Logger.Info("INITIALIZING...");

            // Setup HUDs
            JSON = new Json("Common\\HUDs");

            // Setup the GUI.
            InitializeComponent();
            SetupMenu();
            SetupDirectory();

            // Check for updates.
            AutoUpdater.OpenDownloadPage = true;
            AutoUpdater.Start(Properties.Resources.app_update);
        }

        /// <summary>
        ///     Downloads the latest version of a given HUD.
        /// </summary>
        /// <param name="url">URL from which to download the HUD.</param>
        public static void DownloadHud(string url)
        {
            ServicePointManager.Expect100Continue = true;
            ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;
            ServicePointManager.ServerCertificateValidationCallback = delegate { return true; };
            var client = new WebClient();
            client.DownloadFile(url, "temp.zip");
            client.Dispose();
        }

        /// <summary>
        ///     Check if Team Fortress 2 is currently running.
        /// </summary>
        public static bool CheckGameStatus()
        {
            if (!Process.GetProcessesByName("hl2").Any()) return true;
            ShowMessageBox(MessageBoxImage.Warning, Properties.Resources.info_game_running);
            return false;
        }

        /// <summary>
        ///     Setup the main page and change to the previously selected HUD.
        /// </summary>
        public void SetupMenu()
        {
            gbSelectHUD.Visibility = Visibility.Hidden;
            uiFlawHud.Visibility = Visibility.Hidden;
            uiRaysHud.Visibility = Visibility.Hidden;

            switch (Settings.Default.hud_selected)
            {
                case "flawhud":
                    uiFlawHud.Visibility = Visibility.Visible;
                    break;

                case "rayshud":
                    uiRaysHud.Visibility = Visibility.Visible;
                    break;

                default:
                    gbSelectHUD.Visibility = Visibility.Visible;
                    break;
            }

            // Reload the HUD selection list.
            lbSelectHud.Items.Clear();
            foreach (var HUDItem in JSON.HUDs) lbSelectHud.Items.Add(HUDItem.Name);
            foreach (Enum item in Enum.GetValues(typeof(Common.HUDs)))
                lbSelectHud.Items.Add(Utilities.GetStringValue(item));
        }

        /// <summary>
        ///     Setup the tf/custom directory, if it's not already set.
        /// </summary>
        /// <param name="userSet">Flags the process as being user initiated, skip right to the folder browser.</param>
        public void SetupDirectory(bool userSet = false)
        {
            if (!SearchRegistry() && !CheckUserPath() || userSet)
            {
                // Setup the folder browser.
                Logger.Info("Setting the tf/custom directory. Opening folder browser, asking the user.");
                using (var browser = new FolderBrowserDialog
                {
                    Description = Properties.Resources.info_folder_browser,
                    UseDescriptionForTitle = true,
                    ShowNewFolderButton = true
                })
                {
                    // Loop until the user provides a directory with tf/custom, unless they cancel out.
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

                // Check one more time if a valid directory has been set.
                if (!CheckUserPath())
                {
                    Logger.Error("Unable to set the tf/custom directory. Exiting.");
                    ShowMessageBox(MessageBoxImage.Warning, Properties.Resources.error_app_directory);
                    Application.Current.Shutdown();
                }
            }

            CleanDirectory();
            SetFormControls();
        }

        /// <summary>
        ///     Cleans up the tf/custom and editor directories.
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
                // Remove the previous backup, if found.
                if (File.Exists(HudPath + $"\\{HudSelection}-backup.zip"))
                    File.Delete(HudPath + $"\\{HudSelection}-backup.zip");

                // Create a backup of an existing HUD installation.
                Logger.Info($"Found a {HudSelection} installation. Creating a back-up.");
                ZipFile.CreateFromDirectory(hudDirectory, HudPath + $"\\{HudSelection}-backup.zip");
                Directory.Delete(hudDirectory, true);
                ShowMessageBox(MessageBoxImage.Information,
                    string.Format(Properties.Resources.info_create_backup, HudSelection));
            }

            Logger.Info("Cleaning-up HUD directories...Done!");
        }

        /// <summary>
        ///     Search registry for the Team Fortress 2 installation directory.
        /// </summary>
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
        ///     Check if the currently selected HUD is installed in the tf/custom directory.
        /// </summary>
        public bool CheckHudPath()
        {
            return Directory.Exists(HudPath + "\\" + HudSelection);
        }

        /// <summary>
        ///     Check if the set tf/custom directory is valid.
        /// </summary>
        public static bool CheckUserPath()
        {
            return !string.IsNullOrWhiteSpace(HudPath) && HudPath.Contains("tf\\custom");
        }

        /// <summary>
        ///     Update editor controls, such as labels and buttons.
        /// </summary>
        public void SetFormControls()
        {
            // If the HUD Selection is visible, disable most control buttons.
            if (gbSelectHUD.Visibility == Visibility.Visible)
            {
                BtnInstall.IsEnabled = false;
                BtnUninstall.IsEnabled = false;
                BtnSave.IsEnabled = false;
                BtnReset.IsEnabled = false;
                return;
            }

            if (Directory.Exists(HudPath) && CheckUserPath())
            {
                // The selected HUD is installed in a valid directory.
                var isInstalled = CheckHudPath();
                BtnInstall.IsEnabled = true;
                BtnInstall.Content = isInstalled ? "Reinstall" : "Install";
                BtnUninstall.IsEnabled = isInstalled;
                BtnSave.IsEnabled = isInstalled;
                BtnReset.IsEnabled = isInstalled;
                LblStatus.Content = $"{HudSelection} is {(!isInstalled ? "not " : "")}installed...";
            }
            else
            {
                // The selected HUD is not installed in a valid directory.
                BtnInstall.Content = "Set Directory";
                BtnUninstall.IsEnabled = false;
                BtnSave.IsEnabled = false;
                BtnReset.IsEnabled = false;
                LblStatus.Content = "Directory is not set...";
            }
        }

        /// <summary>
        ///     Display a message box and log the exception message.
        /// </summary>
        public static void ShowMessageBox(MessageBoxImage type, string message, string exception = null)
        {
            MessageBox.Show($"{message} {exception}", string.Empty, MessageBoxButton.OK, type);
            if (!string.IsNullOrEmpty(exception)) Logger.Debug(exception);
        }

        /// <summary>
        ///     Invokes HUD installation or setting the tf/custom directory, if not already set.
        /// </summary>
        private void BtnInstall_OnClick(object sender, RoutedEventArgs e)
        {
            try
            {
                // Force the user to set a directory before installing.
                if (!CheckUserPath())
                {
                    Logger.Info("Opening Directory Browser...");
                    SetupDirectory(true);
                    return;
                }

                // Check if Team Fortress 2 is running before proceeding.
                if (!CheckGameStatus()) return;

                var worker = new BackgroundWorker();
                worker.DoWork += (_, _) =>
                {
                    Dispatcher.Invoke(() =>
                    {
                        // Step 1. Download the HUD
                        Logger.Info($"Downloading the latest {HudSelection}...");
                        switch (HudSelection)
                        {
                            case "flawhud":
                                DownloadHud(Properties.Resources.download_flawhud);
                                break;

                            case "rayshud":
                                DownloadHud(Properties.Resources.download_rayshud);
                                break;
                        }

                        // Step 2. Extract the HUD
                        Logger.Info($"Extracting {HudSelection} to {HudPath}");
                        ZipFile.ExtractToDirectory(Directory.GetCurrentDirectory() + "\\temp.zip", HudPath);

                        // Step 3. Clear out tf/custom of other installed HUDs.
                        foreach (var x in lbSelectHud.Items)
                            if (Directory.Exists(HudPath + "\\" + x.ToString().ToLowerInvariant()))
                                Directory.Delete(HudPath + "\\" + x.ToString().ToLowerInvariant(), true);

                        // Step 4. Update the HUD name by removing the -master suffix.
                        if (Directory.Exists(HudPath + $"\\{HudSelection}-master"))
                            Directory.Move(HudPath + $"\\{HudSelection}-master", HudPath + $"\\{HudSelection}");

                        // Step 5. Clean up the directories and apply user settings.
                        CleanDirectory();

                        switch (HudSelection)
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
                    ShowMessageBox(MessageBoxImage.Information,
                        string.Format(Properties.Resources.info_install_complete, HudSelection));
                };
                worker.RunWorkerAsync();
            }
            catch (Exception ex)
            {
                ShowMessageBox(MessageBoxImage.Error,
                    string.Format(Properties.Resources.error_app_install, HudSelection), ex.Message);
            }
        }

        /// <summary>
        ///     Invokes HUD deletion from the tf/custom directory.
        /// </summary>
        private void BtnUninstall_OnClick(object sender, RoutedEventArgs e)
        {
            try
            {
                // Check if the HUD is installed in a valid directory.
                if (!CheckHudPath()) return;

                // Check if Team Fortress 2 is running before proceeding.
                if (!CheckGameStatus()) return;

                // Uninstall the HUD
                Logger.Info($"Uninstalling {HudSelection}...");
                Directory.Delete(HudPath + $"\\{HudSelection}", true);
                LblStatus.Content = $"Uninstalled {HudSelection} at " + DateTime.Now;
                SetupDirectory();
                ShowMessageBox(MessageBoxImage.Information,
                    string.Format(Properties.Resources.info_uninstall_complete, HudSelection));
            }
            catch (Exception ex)
            {
                ShowMessageBox(MessageBoxImage.Error,
                    string.Format(Properties.Resources.error_app_uninstall, HudSelection), ex.Message);
            }
        }

        /// <summary>
        ///     Saves and applies user selected settings for the given HUD.
        /// </summary>
        private void BtnSave_OnClick(object sender, RoutedEventArgs e)
        {
            Logger.Info("Applying HUD Settings...");
            var worker = new BackgroundWorker();
            worker.DoWork += (_, _) =>
            {
                Dispatcher.Invoke(() =>
                {
                    switch (HudSelection)
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
            //worker.RunWorkerCompleted();  // TODO: Is this needed?
            worker.RunWorkerAsync();
            LblStatus.Content = "Settings Applied at " + DateTime.Now;
        }

        /// <summary>
        ///     Resets settings of the selected HUD.
        /// </summary>
        private void BtnReset_OnClick(object sender, RoutedEventArgs e)
        {
            // Reset settings of the selected HUD.
            switch (HudSelection)
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

        /// <summary>
        ///     Opens the issue tracker for the editor.
        /// </summary>
        private void BtnReportIssue_OnClick(object sender, RoutedEventArgs e)
        {
            Utilities.OpenWebpage(Properties.Resources.app_tracker);
        }

        /// <summary>
        ///     Goes back to the HUD Selection view.
        /// </summary>
        private void BtnSwitch_OnClick(object sender, RoutedEventArgs e)
        {
            lbSelectHud.UnselectAll();
            gbSelectHUD.Visibility = Visibility.Visible;
            uiFlawHud.Visibility = Visibility.Hidden;
            uiRaysHud.Visibility = Visibility.Hidden;
            HUDEditorContainer.Children.Clear();
            SetFormControls();
        }

        private void lbSelectHud_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (lbSelectHud.SelectedIndex == -1) return;
            // Save the selected HUD.
            Settings.Default.hud_selected = lbSelectHud.SelectedValue.ToString().ToLowerInvariant();
            Settings.Default.Save();
            HudSelection = Settings.Default.hud_selected;

            gbSelectHUD.Visibility = Visibility.Hidden;
            HUDEditorContainer.Children.Clear();

            // Change the page view to the selected HUD.
            switch (Settings.Default.hud_selected)
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
                    try
                    {
                        var SelectedHUD = JSON.GetHUDByName(Settings.Default.hud_selected);
                        HUDEditorContainer.Children.Clear();
                        HUDEditorContainer.Children.Add(SelectedHUD.GetControls());
                    }
                    catch (Exception error)
                    {
                        MessageBox.Show(error.ToString());
                    }

                    break;
            }

            // Update the control buttons.
            SetFormControls();
        }
    }
}