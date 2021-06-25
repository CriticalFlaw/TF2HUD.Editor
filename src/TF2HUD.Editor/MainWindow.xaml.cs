using System;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Reflection;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Forms;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using AutoUpdaterDotNET;
using HUDEditor.Classes;
using HUDEditor.Properties;
using log4net;
using log4net.Config;
using Application = System.Windows.Application;
using MessageBox = System.Windows.MessageBox;

namespace HUDEditor
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
            Logger.Info("=======================================================");
            Logger.Info("Initializing.");
            InitializeComponent();

            // Get the list of HUD JSONs.
            Json = new Json();
            LbSelectHud.Items.Clear();
            var list = "Loading list of HUDs";
            foreach (var hud in Json.HUDList)
            {
                list += $" - {hud.Name}";
                LbSelectHud.Items.Add(hud.Name);
            }
            Logger.Info(list);

            // Setup the GUI.
            SetPageView();
            SetPageBackground();
            SetupDirectory();

            // Check for app updates.
            Logger.Info("Checking for app updates.");
            AutoUpdater.OpenDownloadPage = true;
            AutoUpdater.Start(Properties.Resources.app_update);
        }

        /// <summary>
        ///     Setup the tf/custom directory, if it's not already set.
        /// </summary>
        /// <param name="userSet">Flags the process as being user initiated, skip right to the folder browser.</param>
        public void SetupDirectory(bool userSet = false)
        {
            if (!Utilities.SearchRegistry() && !Utilities.CheckUserPath(HudPath) || userSet)
            {
                // Display a folder browser, ask the user to provide the tf/custom directory.
                Logger.Info("tf/custom directory is not set. Asking the user...");
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
                            Logger.Info("tf/custom directory is set to: " + Settings.Default.hud_directory);
                        }
                        else
                        {
                            break;
                        }
                }

                // Check one more time if a valid directory has been set.
                if (!Utilities.CheckUserPath(HudPath))
                {
                    Logger.Info("tf/custom directory still not set. Exiting...");
                    ShowMessageBox(MessageBoxImage.Warning, Properties.Resources.error_app_directory);
                    Application.Current.Shutdown();
                }
            }

            SetPageControls();
        }

        #region PAGE_EVENTS

        /// <summary>
        ///     Update editor controls, such as labels and buttons.
        /// </summary>
        public void SetPageControls()
        {
            // Clean the application directory.
            if (File.Exists(Directory.GetCurrentDirectory() + "\\temp.zip"))
                File.Delete(Directory.GetCurrentDirectory() + "\\temp.zip");

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

            if (Directory.Exists(HudPath) && Utilities.CheckUserPath(HudPath))
            {
                // The selected HUD is installed in a valid directory.
                var isInstalled = CheckHudInstallation();
                BtnInstall.IsEnabled = true;
                BtnInstall.Content = isInstalled ? "Reinstall" : "Install";
                BtnUninstall.IsEnabled = isInstalled;
                BtnSave.IsEnabled = isInstalled;
                BtnReset.IsEnabled = isInstalled;
                BtnSwitch.IsEnabled = true;
                LblStatus.Content = $"{HudSelection} is {(!isInstalled ? "not " : "")}installed!";
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
                LblStatus.Content = "tf/custom directory is not set!";
            }
            Logger.Info(LblStatus.Content);
        }

        /// <summary>
        ///     Display a message box with a message to the user and log it.
        /// </summary>
        public static MessageBoxResult ShowMessageBox(MessageBoxImage type, string message,
            MessageBoxButton buttons = MessageBoxButton.OK)
        {
            if (type == MessageBoxImage.Error)
                Logger.Error(message);
            else if (type == MessageBoxImage.Warning)
                Logger.Warn(message);

            return MessageBox.Show(message, string.Empty, buttons, type);
        }


        /// <summary>
        ///     Update the page view to the selected HUD.
        /// </summary>
        private void SetPageView()
        {
            try
            {
                var selection = Json.GetHUDByName(Settings.Default.hud_selected);

                // Display a list of available HUDs if a HUD selection has not been set.
                GbSelectHud.Visibility = (selection is null) ? Visibility.Visible : Visibility.Hidden;
                EditorContainer.Children.Clear();

                // If there's a HUD selection, generate the controls for that HUD's page.
                if (selection is null) return;
                Logger.Info($"Changing page view to: {selection.Name}.");
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
            var selection = Json.GetHUDByName(Settings.Default.hud_selected);
            if (selection?.Background == null) return;

            Logger.Info($"Changing background to: {selection.Background}");
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

        /// <summary>
        ///     Check if the selected hud is installed in tf/custom.
        /// </summary>
        /// <returns>True if the selected hud is installed.</returns>
        public static bool CheckHudInstallation()
        {
            return Directory.Exists(HudPath + "\\" + HudSelection);
        }

        #endregion

        #region CLICK_EVENTS

        /// <summary>
        ///     Invoke HUD installation or setting the tf/custom directory, if not already set.
        /// </summary>
        private void BtnInstall_OnClick(object sender, RoutedEventArgs e)
        {
            try
            {
                // Force the user to set a directory before installing.
                if (!Utilities.CheckUserPath(HudPath))
                {
                    SetupDirectory(true);
                    return;
                }

                // Stop the process if Team Fortress 2 is still running.
                if (Utilities.CheckIsGameRunning(true)) return;

                var worker = new BackgroundWorker();
                worker.DoWork += (_, _) =>
                {
                    Dispatcher.Invoke(() =>
                    {
                        // Step 1. Retrieve the HUD object, then download and extract it into the tf/custom directory.
                        Logger.Info($"Start installing {HudSelection}.");
                        Json.GetHUDByName(Settings.Default.hud_selected).Update();

                        // Step 2. Clear tf/custom directory of other installed HUDs.
                        Logger.Info("Preparing directories for extraction.");
                        foreach (var x in LbSelectHud.Items)
                            if (Directory.Exists(HudPath + "\\" + x.ToString()?.ToLowerInvariant()))
                                Directory.Delete(HudPath + "\\" + x.ToString()?.ToLowerInvariant(), true);

                        // Step 3. Record the name of the HUD inside the downloaded folder.
                        var tempFile = $"{AppDomain.CurrentDomain.BaseDirectory}temp.zip";
                        if (!File.Exists(tempFile)) tempFile = "temp.zip";
                        using var archive = ZipFile.OpenRead(tempFile);
                        var hudName = archive.Entries.FirstOrDefault(entry =>
                            entry.FullName.EndsWith("/", StringComparison.OrdinalIgnoreCase) &&
                            string.IsNullOrWhiteSpace(entry.Name));

                        // Step 4. Extract the downloaded HUD and rename it to match the schema.
                        Logger.Info($"Extracting {HudSelection} to: {HudPath}");
                        ZipFile.ExtractToDirectory(tempFile, HudPath);
                        Directory.Move($"{HudPath}\\{hudName}", $"{HudPath}\\{HudSelection}");

                        // Step 5. Update the page view.
                        if (string.IsNullOrWhiteSpace(HudSelection)) return;
                        var selection = Json.GetHUDByName(Settings.Default.hud_selected);
                        selection.Settings.SaveSettings();
                        EditorContainer.Children.Clear();
                        EditorContainer.Children.Add(selection.GetControls());
                    });
                };
                worker.RunWorkerCompleted += (_, _) =>
                {
                    LblStatus.Content = "Installation finished at " + DateTime.Now;
                    ShowMessageBox(MessageBoxImage.Information,
                        string.Format(Properties.Resources.info_install_complete, HudSelection));
                    Json.GetHUDByName(Settings.Default.hud_selected).ApplyCustomizations();
                    LblStatus.Content = $"Settings Applied at {DateTime.Now}";
                    SetPageControls();
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
                if (!CheckHudInstallation()) return;

                // Stop the process if Team Fortress 2 is still running.
                if (Utilities.CheckIsGameRunning(true)) return;

                // Remove the HUD from the tf/custom directory.
                Logger.Info($"Start uninstalling {HudSelection}.");
                Logger.Info($"Removing {HudSelection} from: {HudPath}");
                Directory.Delete(HudPath + $"\\{HudSelection}", true);
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

            Logger.Info("Start applying settings.");
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
                    Logger.Info("Done applying settings.");
                });
            };
            worker.RunWorkerAsync();
            LblStatus.Content = $"Settings Applied at {DateTime.Now}";
        }

        /// <summary>
        ///     Reset user settings for the selected HUD to default.
        /// </summary>
        private void BtnReset_OnClick(object sender, RoutedEventArgs e)
        {
            // Ask the user if they want to reset before doing so.
            if (ShowMessageBox(MessageBoxImage.Question, Properties.Resources.ask_reset_options,
                MessageBoxButton.YesNo) != MessageBoxResult.Yes) return;

            Logger.Info("Start resetting settings.");
            var selection = Json.GetHUDByName(Settings.Default.hud_selected);
            selection.ResetAll();
            selection.Settings.SaveSettings();
            selection.ApplyCustomizations();
            selection.DirtyControls.Clear();
            LblStatus.Content = $"Settings Reset at {DateTime.Now}";
            Logger.Info("Done resetting settings.");
        }

        /// <summary>
        ///     Return to the HUD selection page.
        /// </summary>
        private void BtnSwitch_OnClick(object sender, RoutedEventArgs e)
        {
            Logger.Info("Changing page view to: main menu.");
            LbSelectHud.UnselectAll();
            GbSelectHud.Visibility = Visibility.Visible;
            EditorContainer.Children.Clear();
            SetPageControls();
            SetPageBackground();
        }

        private void BtnSetDirectory_OnClick(object sender, RoutedEventArgs e)
        {
            Logger.Info("Attempting to change the 'tf/custom' directory.");
            var previousPath = HudPath;
            SetupDirectory(true);

            if (HudPath.Equals(previousPath) || !Utilities.CheckUserPath(HudPath))
                ShowMessageBox(MessageBoxImage.Error, Properties.Resources.info_path_incorrect);
            else
                ShowMessageBox(MessageBoxImage.Information, Properties.Resources.info_path_correct);
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
            // Check for HUD updates.
            Logger.Info("Checking for schema updates.");
            Json.UpdateAsync().ContinueWith(restartRequired =>
            {
                if (!restartRequired.Result)
                {
                    MessageBox.Show(Properties.Resources.info_schema_nothing, "No Updates Found.", MessageBoxButton.OK, MessageBoxImage.Information);
                    return;
                }

                if (MessageBox.Show(Properties.Resources.info_schema_update, "Restart Required", MessageBoxButton.YesNo, MessageBoxImage.Information) != MessageBoxResult.Yes) return;

                Json.Update(true);
                Debug.WriteLine(Assembly.GetExecutingAssembly().Location);
                Process.Start(Assembly.GetExecutingAssembly().Location.Replace(".dll", ".exe"));
                Environment.Exit(0);
            });
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