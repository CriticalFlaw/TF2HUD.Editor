using System;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Net;
using System.Reflection;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Forms;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using AutoUpdaterDotNET;
using log4net;
using log4net.Config;
using Microsoft.Win32;
using TF2HUD.Editor.Classes;
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
        public Json Json;

        public MainWindow()
        {
            // Initialize the logger.
            var repository = LogManager.GetRepository(Assembly.GetEntryAssembly());
            XmlConfigurator.Configure(repository, new FileInfo("log4net.config"));
            InitializeComponent();

            // Get the list of HUD JSONs.
            Json = new Json();
            LbSelectHud.Items.Clear();
            foreach (var hud in Json.HUDList) LbSelectHud.Items.Add(hud.Name);

            // Setup the GUI.
            SetPageView();
            SetPageBackground();
            SetupDirectory();

            // Check for app updates.
            Logger.Info("Checking for updates.");
            AutoUpdater.OpenDownloadPage = true;
            AutoUpdater.Start(Properties.Resources.app_update);

            // Check for HUD updates.
            Json.UpdateAsync().ContinueWith((Task<bool> restartRequired) =>
            {
                if (restartRequired.Result)
                {
                    var result = MessageBox.Show("Application restart required to update HUD schemas, would you like to restart now?", "Restart Required", MessageBoxButton.YesNo, MessageBoxImage.Information);
                    if(result == MessageBoxResult.Yes)
                    {
                        System.Diagnostics.Debug.WriteLine(Assembly.GetExecutingAssembly().Location);
                        Process.Start(Assembly.GetExecutingAssembly().Location.Replace(".dll", ".exe"));
                        Environment.Exit(0);
                    }
                }
            });
        }

        /// <summary>
        ///     Download the HUD using the provided URL.
        /// </summary>
        /// <param name="url">URL from which to download the HUD.</param>
        public static void DownloadHud(string url)
        {
            Logger.Info($"Downloading from: {url}");
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
        /// <returns>False if there's no active process named hl2, otherwise return true and a warning message.</returns>
        public static bool CheckIsGameRunning(bool returnMessage = false)
        {
            if (!Process.GetProcessesByName("hl2").Any()) return false;
            if (returnMessage)
                ShowMessageBox(MessageBoxImage.Warning, Properties.Resources.info_game_running);
            return true;
        }

        /// <summary>
        ///     Setup the tf/custom directory, if it's not already set.
        /// </summary>
        /// <param name="userSet">Flags the process as being user initiated, skip right to the folder browser.</param>
        public void SetupDirectory(bool userSet = false)
        {
            if (!SearchRegistry() && !CheckUserPath() || userSet)
            {
                // Display a folder browser, ask the user to provide the tf/custom directory.
                Logger.Info("Opening a folder browser to get the tf/custom directory from the user.");
                using (var browser = new FolderBrowserDialog
                {
                    Description = Properties.Resources.ask_folder_browser,
                    UseDescriptionForTitle = true,
                    ShowNewFolderButton = true
                })
                {
                    // Loop until the user provides a valid tf/custom directory, unless they cancel out.
                    while (!browser.SelectedPath.EndsWith("tf\\custom"))
                        if (browser.ShowDialog() == System.Windows.Forms.DialogResult.OK &&
                            browser.SelectedPath.EndsWith("tf\\custom"))
                        {
                            Settings.Default.hud_directory = browser.SelectedPath;
                            Settings.Default.Save();
                            HudPath = Settings.Default.hud_directory;
                            Logger.Info("tf/custom directory has been set to: " + Settings.Default.hud_directory);
                        }
                        else
                        {
                            break;
                        }
                }

                // Check one more time if a valid directory has been set.
                if (!CheckUserPath())
                {
                    Logger.Info("Unable to set the tf/custom directory. Exiting.");
                    ShowMessageBox(MessageBoxImage.Warning, Properties.Resources.error_app_directory);
                    Application.Current.Shutdown();
                }
            }

            CleanDirectory();
            SetPageControls();
        }

        /// <summary>
        ///     Cleans up the tf/custom and editor directories.
        /// </summary>
        private static void CleanDirectory()
        {
            // Clean the application directory.
            if (File.Exists(Directory.GetCurrentDirectory() + "\\temp.zip"))
            {
                Logger.Info("Found a temp file from the previous download attempt. Removing.");
                File.Delete(Directory.GetCurrentDirectory() + "\\temp.zip");
            }

            // Clean the tf/custom directory.
            var hudDirectory = Directory.Exists(HudPath + $"\\{HudSelection}-master")
                ? HudPath + $"\\{HudSelection}-master"
                : string.Empty;

            hudDirectory = Directory.Exists(HudPath + $"\\{HudSelection}-main")
                ? HudPath + $"\\{HudSelection}-main"
                : string.Empty;

            // If the tf/custom directory is not yet set, then exit.
            if (string.IsNullOrEmpty(hudDirectory)) return;

            // If found, remove the existing backup file.
            if (File.Exists(HudPath + $"\\{HudSelection}-backup.zip"))
            {
                Logger.Info("Found a backup file from the previous HUD installation. Removing.");
                File.Delete(HudPath + $"\\{HudSelection}-backup.zip");
            }

            // Create a new backup of an existing HUD installation.
            Logger.Info("Found an existing HUD installation. Creating a new backup.");
            ZipFile.CreateFromDirectory(hudDirectory, HudPath + $"\\{HudSelection}-backup.zip");
            Directory.Delete(hudDirectory, true);
            ShowMessageBox(MessageBoxImage.Information,
                string.Format(Properties.Resources.info_create_backup, HudSelection));
        }

        /// <summary>
        ///     Search registry for the Team Fortress 2 installation directory.
        /// </summary>
        /// <returns>True if the TF2 directory was found through the registry, otherwise return False.</returns>
        public static bool SearchRegistry()
        {
            // Try to find the Steam library path in the registry.
            var is64Bit = Environment.Is64BitProcess ? "Wow6432Node\\" : string.Empty;
            var registry = (string) Registry.GetValue($@"HKEY_LOCAL_MACHINE\Software\{is64Bit}Valve\Steam",
                "InstallPath", null);

            if (string.IsNullOrWhiteSpace(registry)) return false;

            // Found the Steam library path, now try to find the TF2 path.
            registry += "\\steamapps\\common\\Team Fortress 2\\tf\\custom";

            if (!Directory.Exists(registry)) return false;

            Logger.Info("Found the TF2 path in the registry at: " + registry);
            Settings.Default.hud_directory = registry;
            Settings.Default.Save();
            return true;
        }

        /// <summary>
        ///     Check if the currently selected HUD is installed in the tf/custom directory.
        /// </summary>
        /// <returns>True if the selected HUD is in the tf/custom directory.</returns>
        public static bool CheckHudPath()
        {
            return Directory.Exists(HudPath + "\\" + HudSelection);
        }

        /// <summary>
        ///     Check if the set tf/custom directory is valid.
        /// </summary>
        /// <returns>True if the set tf/custom directory is valid.</returns>
        public static bool CheckUserPath()
        {
            return !string.IsNullOrWhiteSpace(HudPath) && HudPath.EndsWith("tf\\custom");
        }

        /// <summary>
        ///     Update editor controls, such as labels and buttons.
        /// </summary>
        public void SetPageControls()
        {
            // If the HUD Selection is visible, disable most control buttons.
            if (GbSelectHud.Visibility == Visibility.Visible)
            {
                BtnInstall.IsEnabled = false;
                BtnUninstall.IsEnabled = false;
                BtnSave.IsEnabled = false;
                BtnReset.IsEnabled = false;
                BtnSwitch.IsEnabled = false;
                BtnGitHub.IsEnabled = false;
                BtnHuds.IsEnabled = false;
                BtnDiscord.IsEnabled = false;
                BtnSteam.IsEnabled = false;
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
                BtnSwitch.IsEnabled = true;
                LblStatus.Content = $"{HudSelection} is {(!isInstalled ? "not " : "")}installed...";
            }
            else
            {
                // The selected HUD is not installed in a valid directory.
                BtnInstall.Content = "Set Directory";
                BtnInstall.IsEnabled = true;
                BtnUninstall.IsEnabled = false;
                BtnSave.IsEnabled = false;
                BtnReset.IsEnabled = false;
                BtnSwitch.IsEnabled = true;
                LblStatus.Content = "Directory is not set...";
            }
        }

        /// <summary>
        ///     Display a message box with a message to the user and log it.
        /// </summary>
        public static MessageBoxResult ShowMessageBox(MessageBoxImage type, string message,
            MessageBoxButton buttons = MessageBoxButton.OK)
        {
            if (type == MessageBoxImage.Error)
                Logger.Error(message);
            else
                Logger.Info(message);

            return MessageBox.Show(message, string.Empty, buttons, type);
        }


        /// <summary>
        ///     Update the page view to the selected HUD.
        /// </summary>
        private void SetPageView()
        {
            try
            {
                // Display a list of available HUDs if a HUD selection has not been set.
                GbSelectHud.Visibility = string.IsNullOrWhiteSpace(Settings.Default.hud_selected)
                    ? Visibility.Visible
                    : Visibility.Hidden;
                EditorContainer.Children.Clear();

                // If there's a HUD selection, generate the controls for that HUD's page.
                if (string.IsNullOrWhiteSpace(Settings.Default.hud_selected)) return;
                var selection = Json.GetHUDByName(Settings.Default.hud_selected);
                EditorContainer.Children.Add(selection.GetControls());

                // Maximize the application window if a given HUD schema requests it.
                Application.Current.MainWindow.WindowState =
                    selection.Maximize ? WindowState.Maximized : WindowState.Normal;

                // Disable the social media buttons if they don't have a link.
                BtnGitHub.IsEnabled = !string.IsNullOrWhiteSpace(selection.GitHubUrl);
                BtnHuds.IsEnabled = !string.IsNullOrWhiteSpace(selection.HudsTfUrl);
                BtnDiscord.IsEnabled = !string.IsNullOrWhiteSpace(selection.DiscordUrl);
                BtnSteam.IsEnabled = !string.IsNullOrWhiteSpace(selection.SteamUrl);
            }
            catch (Exception ex)
            {
                ShowMessageBox(MessageBoxImage.Error, ex.Message);
            }
        }

        /// <summary>
        ///     Change the page background to a given color or image, if provided.
        /// </summary>
        private void SetPageBackground()
        {
            // Reset the application to the default background color.
            if (GbSelectHud.Visibility == Visibility.Visible)
            {
                var converter = new BrushConverter();
                MainGrid.Background = (Brush) converter.ConvertFromString("#2B2724");
                return;
            }

            // If a HUD is selected, retrieve the set Background value if available.
            if (string.IsNullOrWhiteSpace(Settings.Default.hud_selected)) return;
            var selection = Json.GetHUDByName(Settings.Default.hud_selected);
            if (selection.Background == null) return;

            if (selection.Background.StartsWith("http"))
            {
                // The Background is a URL, attempt to download and load the image from the Internet.
                MainGrid.Background = new ImageBrush
                {
                    Stretch = Stretch.UniformToFill,
                    Opacity = selection.Opacity,
                    ImageSource = new BitmapImage(new Uri(selection.Background, UriKind.RelativeOrAbsolute))
                };
            }
            else
            {
                // The Background is an RGBA color code, change it to ARGB and set it as the background.
                var colors = Array.ConvertAll(selection.Background.Split(' '), byte.Parse);
                MainGrid.Background = new SolidColorBrush(Color.FromArgb(colors[^1], colors[0], colors[1], colors[2]));
            }
        }

        #region CLICK_EVENTS

        /// <summary>
        ///     Invoke HUD installation or setting the tf/custom directory, if not already set.
        /// </summary>
        private void BtnInstall_OnClick(object sender, RoutedEventArgs e)
        {
            try
            {
                // Force the user to set a directory before installing.
                if (!CheckUserPath())
                {
                    SetupDirectory(true);
                    return;
                }

                // Stop the process if Team Fortress 2 is still running.
                if (CheckIsGameRunning(true)) return;

                var worker = new BackgroundWorker();
                worker.DoWork += (_, _) =>
                {
                    Dispatcher.Invoke(() =>
                    {
                        // Step 1. Retrieve the HUD object, then download and extract it into the tf/custom directory.
                        Logger.Info($"Downloading {HudSelection}...");
                        Json.GetHUDByName(Settings.Default.hud_selected).Update();

                        // Step 2. Clear tf/custom directory of other installed HUDs.
                        foreach (var x in LbSelectHud.Items)
                            if (Directory.Exists(HudPath + "\\" + x.ToString()?.ToLowerInvariant()))
                                Directory.Delete(HudPath + "\\" + x.ToString()?.ToLowerInvariant(), true);

                        // Step 3. Record the name of the HUD inside the downloaded folder.
                        var tempFile = $"{AppDomain.CurrentDomain.BaseDirectory}\\temp.zip";
                        using var archive = ZipFile.OpenRead(tempFile);
                        var hudName = archive.Entries.FirstOrDefault(entry => entry.FullName.EndsWith("/", StringComparison.OrdinalIgnoreCase) && string.IsNullOrWhiteSpace(entry.Name));

                        // Step 4. Extract the downloaded HUD and rename it to match the schema.
                        Logger.Info($"Extracting {HudSelection} into {HudPath}");
                        ZipFile.ExtractToDirectory(tempFile, HudPath);
                        Directory.Move($"{HudPath}\\{hudName}", $"{HudPath}\\{HudSelection}");

                        // Step 5. Clean up, check user settings and reload the page UI.
                        CleanDirectory();

                        if (!string.IsNullOrWhiteSpace(HudSelection))
                        {
                            var selection = Json.GetHUDByName(Settings.Default.hud_selected);
                            selection.Settings.SaveSettings();
                            EditorContainer.Children.Clear();
                            EditorContainer.Children.Add(selection.GetControls());
                        }

                        SetPageControls();
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
                    $"{string.Format(Properties.Resources.error_app_install, HudSelection)} {ex.Message}");
            }
        }

        /// <summary>
        ///     Invoke HUD deletion from the tf/custom directory.
        /// </summary>
        private void BtnUninstall_OnClick(object sender, RoutedEventArgs e)
        {
            try
            {
                // Check if the HUD is installed in a valid directory.
                if (!CheckHudPath()) return;

                // Stop the process if Team Fortress 2 is still running.
                if (CheckIsGameRunning(true)) return;

                // Remove the HUD from the tf/custom directory.
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
                    $"{string.Format(Properties.Resources.error_app_uninstall, HudSelection)} {ex.Message}");
            }
        }

        /// <summary>
        ///     Save and apply user settings to the HUD files.
        /// </summary>
        private void BtnSave_OnClick(object sender, RoutedEventArgs e)
        {
            var hudObject = Json.GetHUDByName(Settings.Default.hud_selected);
            if (Process.GetProcessesByName("hl2").Any() && hudObject.DirtyControls.Count > 0)
            {
                var message = hudObject.DirtyControls.Aggregate(Properties.Resources.info_game_restart,
                    (current, control) => current + $"\n - {control}");
                if (ShowMessageBox(MessageBoxImage.Question, message) != MessageBoxResult.OK) return;
            }

            Logger.Info("Applying HUD Settings...");
            var worker = new BackgroundWorker();
            worker.DoWork += (_, _) =>
            {
                Dispatcher.Invoke(() =>
                {
                    if (string.IsNullOrWhiteSpace(HudSelection)) return;
                    var selection = Json.GetHUDByName(Settings.Default.hud_selected);
                    selection.Settings.SaveSettings();
                    selection.ApplyCustomizations();
                    selection.DirtyControls.Clear();
                });
            };
            worker.RunWorkerAsync();
            LblStatus.Content = "Settings Applied at " + DateTime.Now;
            Logger.Info("Applying HUD Settings...Done!");
        }

        /// <summary>
        ///     Reset user settings for the selected HUD to default.
        /// </summary>
        private void BtnReset_OnClick(object sender, RoutedEventArgs e)
        {
            // Ask the user if they want to reset before doing so.
            if (ShowMessageBox(MessageBoxImage.Question, Properties.Resources.ask_reset_options,
                MessageBoxButton.YesNo) != MessageBoxResult.Yes) return;

            Logger.Info("Resetting HUD Settings...");
            var selection = Json.GetHUDByName(Settings.Default.hud_selected);
            selection.Reset();
            selection.Settings.SaveSettings();
            selection.ApplyCustomizations();
            selection.DirtyControls.Clear();
            LblStatus.Content = "Settings Reset at " + DateTime.Now;
            Logger.Info("Resetting HUD Settings...Done!");
        }

        /// <summary>
        ///     Return to the HUD selection page.
        /// </summary>
        private void BtnSwitch_OnClick(object sender, RoutedEventArgs e)
        {
            LbSelectHud.UnselectAll();
            GbSelectHud.Visibility = Visibility.Visible;
            EditorContainer.Children.Clear();
            SetPageControls();
            SetPageBackground();
        }

        /// <summary>
        ///     Opens the issue tracker for the editor.
        /// </summary>
        private void BtnReportIssue_OnClick(object sender, RoutedEventArgs e)
        {
            Utilities.OpenWebpage(Properties.Resources.app_tracker);
        }

        /// <summary>
        ///     Opens the project documentation site.
        /// </summary>
        private void BtnDocumentation_OnClick(object sender, RoutedEventArgs e)
        {
            Utilities.OpenWebpage(Properties.Resources.app_docs);
        }

        /// <summary>
        ///     Updates the local schema files to the latest version.
        /// </summary>
        private void BtnRefresh_OnClick(object sender, RoutedEventArgs e)
        {
            LblStatus.Content = Json.Update()
                ? Properties.Resources.info_schema_update
                : Properties.Resources.info_schema_nothing;
        }

        private void BtnSteam_OnClick(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(Settings.Default.hud_selected)) return;
            Utilities.OpenWebpage(Json.GetHUDByName(Settings.Default.hud_selected).SteamUrl);
        }

        private void BtnGitHub_OnClick(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(Settings.Default.hud_selected)) return;
            Utilities.OpenWebpage(Json.GetHUDByName(Settings.Default.hud_selected).GitHubUrl);
        }

        private void BtnHuds_OnClick(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(Settings.Default.hud_selected)) return;
            Utilities.OpenWebpage(Json.GetHUDByName(Settings.Default.hud_selected).HudsTfUrl);
        }

        private void BtnDiscord_OnClick(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(Settings.Default.hud_selected)) return;
            Utilities.OpenWebpage(Json.GetHUDByName(Settings.Default.hud_selected).DiscordUrl);
        }

        /// <summary>
        ///     Save the selected HUD then update the page view and controls.
        /// </summary>
        private void lbSelectHud_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            // Break if the selection is invalid.
            if (LbSelectHud.SelectedIndex == -1) return;

            // Save the selected HUD.
            Settings.Default.hud_selected = LbSelectHud.SelectedValue.ToString()?.ToLowerInvariant();
            Settings.Default.Save();
            HudSelection = Settings.Default.hud_selected;

            // Change the page view and update the controls.
            SetPageView();
            SetPageBackground();
            SetPageControls();
        }

        #endregion
    }
}