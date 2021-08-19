using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Reflection;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Forms;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using AutoUpdaterDotNET;
using HUDEditor.Classes;
using HUDEditor.Properties;
using log4net;
using log4net.Config;
using TF2HUDEditor.Classes;
using WPFLocalizeExtension.Engine;
using Application = System.Windows.Application;
using Binding = System.Windows.Data.Binding;
using Label = System.Windows.Controls.Label;
using MessageBox = System.Windows.MessageBox;

namespace HUDEditor
{
    /// <summary>
    ///     Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow
    {
        public static string HudSelection = Settings.Default.hud_selected;

        // Path to the user tf/custom folder
        public static string HudPath = Settings.Default.hud_directory;
        public static readonly ILog Logger = LogManager.GetLogger(MethodBase.GetCurrentMethod()?.DeclaringType);
        private readonly List<(HUD, Border)> HudThumbnails = new();

        public MainWindow()
        {
            // Initialize the logger.
            var repository = LogManager.GetRepository(Assembly.GetEntryAssembly());
            XmlConfigurator.Configure(repository, new FileInfo("log4net.config"));
            Logger.Info("=======================================================");
            Logger.Info("Initializing.");

            // Get the list of HUD JSONs.
            Json = new Json();

            InitializeComponent();
            this.DataContext = this;

            var list = "Loading list of HUDs";
            foreach (var hud in Json.HUDList)
            {
                list += $" - {hud.Name}";

                var border = new Border();
                border.MouseEnter += (_, _) => border.BorderBrush = Brushes.Black;
                border.MouseLeave += (_, _) => border.BorderBrush = Brushes.LightGray;
                border.MouseUp += (object sender, MouseButtonEventArgs e) =>
                {
                    if (Json.HighlightedHUD == hud)
                    {
                        Json.SelectedHUD = hud;
                    }
                    else
                    {
                        Json.SelectedHUD = null;
                        Json.HighlightedHUD = hud;
                        HudInfo.ScrollToVerticalOffset(0);
                        Logger.Info($"Highlighting {hud.Name}");
                    }
                };

                border.Style = (Style)Application.Current.Resources["HudListBorder"];

                var thumbnailImage = new Image();
                if (hud.Thumbnail != null) thumbnailImage.Source = new BitmapImage(new Uri(hud.Thumbnail));
                thumbnailImage.Style = (Style)Application.Current.Resources["HudListImage"];

                // Prevents image from looking grainy when resized
                RenderOptions.SetBitmapScalingMode(thumbnailImage, BitmapScalingMode.Fant);

                var hudIconContainer = new StackPanel();
                hudIconContainer.Children.Add(thumbnailImage);
                hudIconContainer.Children.Add(new Label
                    { Content = hud.Name, Style = (Style)Application.Current.Resources["HudListLabel"] });

                border.Child = hudIconContainer;

                // Automatically assign columns and rows
                // Grid.SetColumn(border, GridSelectHUD.Children.Count % 2);
                // int d = GridSelectHUD.Children.Count / 2;
                // Grid.SetRow(border, d);

                GridSelectHud.Children.Add(border);
                HudThumbnails.Add((hud, border));
            }

            Logger.Info(list);

            // Setup the GUI.
            SetPageView(Json.SelectedHUD);
            SelectionChanged(Json, Json.GetHUDByName(Settings.Default.hud_selected));
            SetupDirectory();

            Json.SelectionChanged += SelectionChanged;

            // Check for app updates.
            Logger.Info("Checking for app updates.");
            AutoUpdater.OpenDownloadPage = true;
            AutoUpdater.Start(Settings.Default.app_update);
        }

        public static Json Json { get; set; }

        /// <summary>
        ///     Setup the tf/custom directory, if it's not already set.
        /// </summary>
        /// <param name="userSet">Flags the process as being user initiated, skip right to the folder browser.</param>
        public void SetupDirectory(bool userSet = false)
        {
            if ((Utilities.SearchRegistry() || Utilities.CheckUserPath(HudPath)) && !userSet) return;
            // Display a folder browser, ask the user to provide the tf/custom directory.
            Logger.Info("tf/custom directory is not set. Asking the user...");
            using (var browser = new FolderBrowserDialog
            {
                Description = Properties.Resources.info_path_browser,
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
            if (Utilities.CheckUserPath(HudPath)) return;
            Logger.Info("tf/custom directory still not set. Exiting...");
            ShowMessageBox(MessageBoxImage.Warning,
                Utilities.GetLocalizedString(Properties.Resources.error_app_directory));
            Application.Current.Shutdown();
        }

        public void SelectionChanged(object sender, HUD hud)
        {
            if (hud != null)
            {
                // Save the selected HUD.
                HudSelection = hud.Name;
                if (Json.SelectedHUDInstalled)
                {
                    LblStatus.Content = string.Format(Properties.Resources.status_installed, Json.SelectedHUD.Name);
                }
                else if (Directory.Exists(HudPath))
                {
                    LblStatus.Content = string.Format(Properties.Resources.status_installed_not, Json.SelectedHUD.Name);
                }
                else
                {
                    LblStatus.Content = Properties.Resources.status_pathNotSet;
                }

                Application.Current.MainWindow.WindowState = hud.Maximize ? WindowState.Maximized : WindowState.Normal;

                Settings.Default.hud_selected = hud.Name;
                Settings.Default.Save();
            }
            else
            {
                HudSelection = string.Empty;
                LblStatus.Content = string.Empty;
            }

            SetPageView(hud);
        }

        #region PAGE_EVENTS

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
        private void SetPageView(HUD selection)
        {
            try
            {
                EditorContainer.Children.Clear();

                // If there's a HUD selection, generate the controls for that HUD's page.
                if (selection is null) return;

                Logger.Info($"Changing page view to: {selection.Name}.");

                EditorContainer.Children.Add(selection.GetControls());
                selection.PresetChanged += (_, _) =>
                {
                    EditorContainer.Children.Clear();
                    EditorContainer.Children.Add(selection.GetControls());
                };
            }
            catch (Exception ex)
            {
                ShowMessageBox(MessageBoxImage.Error, ex.Message);
            }
        }

        /// <summary>
        ///     Check if the selected hud is installed in tf/custom.
        /// </summary>
        /// <returns>True if the selected hud is installed.</returns>
        public static bool CheckHudInstallation()
        {
            return Directory.Exists($"{HudPath}\\{HudSelection}");
        }

        #endregion

        #region CLICK_EVENTS

        private void TbSearchBox_TextChanged(object sender, RoutedEventArgs e)
        {
            var searchText = SearchBox.Text.ToLower();
            foreach (var (hud, border) in HudThumbnails)
            {
                var visibility = Visibility.Collapsed;

                var searches = new[]
                {
                    hud.Name,
                    hud.Author,
                    hud.Description
                };

                var i = 0;
                while (visibility == Visibility.Collapsed && i < searches.Length)
                {
                    if (searches[i] != null && searches[i].ToLower().Contains(searchText))
                        visibility = Visibility.Visible;
                    i++;
                }

                border.Visibility = visibility;
            }
        }

        /// <summary>
        ///     Invoke HUD installation or setting the tf/custom directory, if not already set.
        /// </summary>
        private void BtnInstall_OnClick(object sender, RoutedEventArgs e)
        {
            try
            {
                // Disable switching HUD while installing to ensure that HighlightedHUD
                // is the same as SelectedHUD at worker.RunWorkerCompleted
                BtnSwitch.IsEnabled = false;

                Json.SelectedHUD = Json.HighlightedHUD;
                Settings.Default.hud_selected = HudSelection = Json.SelectedHUD.Name;

                // Force the user to set a directory before installing.
                if (!Utilities.CheckUserPath(HudPath))
                {
                    SetupDirectory(true);
                    // return;
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
                        Json.SelectedHUD.Update();

                        // Step 2. Clear tf/custom directory of other installed HUDs.
                        Logger.Info("Preparing directories for extraction.");
                        foreach (var x in Json.HUDList)
                            if (Directory.Exists(HudPath + "\\" + x.Name?.ToLowerInvariant()))
                                Directory.Delete(HudPath + "\\" + x.Name?.ToLowerInvariant(), true);

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
                        Json.SelectedHUD.Settings.SaveSettings();
                        SetPageView(Json.SelectedHUD);
                        Json.SelectedHUD.ApplyCustomizations();
                    });
                };
                worker.RunWorkerCompleted += (_, _) =>
                {
                    LblStatus.Content = string.Format(Properties.Resources.status_installed_now,
                        Settings.Default.hud_selected, DateTime.Now);

                    // Update Install/Uninstall/Reset Buttons
                    Json.OnPropertyChanged("HighlightedHUDInstalled");

                    // Update Switch HUD Button
                    BtnSwitch.SetBinding(
                        IsEnabledProperty,
                        new Binding
                        {
                            Source = Json,
                            Path = new PropertyPath("SelectedHUD"),
                            Converter = new NullCheckConverter()
                        }
                    );
                    Json.OnPropertyChanged("SelectedHUD");

                    // Clean the application directory.
                    var tempPath = $"{Directory.GetCurrentDirectory()}\\temp.zip";
                    if (File.Exists(tempPath))
                        File.Delete(tempPath);
                };
                worker.RunWorkerAsync();
            }
            catch (Exception ex)
            {
                ShowMessageBox(MessageBoxImage.Error,
                    $"{string.Format(Utilities.GetLocalizedString(Properties.Resources.error_hud_install), HudSelection)} {ex.Message}");
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
                // SetupDirectory();

                Json.OnPropertyChanged("HighlightedHUD");
                Json.OnPropertyChanged("HighlightedHUDInstalled");
            }
            catch (Exception ex)
            {
                ShowMessageBox(MessageBoxImage.Error,
                    $"{string.Format(Utilities.GetLocalizedString(Properties.Resources.error_hud_uninstall), HudSelection)} {ex.Message}");
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
            worker.RunWorkerCompleted += (_, _) =>
            {
                LblStatus.Content =
                    string.Format(Properties.Resources.status_applied, hudObject.Name, DateTime.Now);
            };
            worker.RunWorkerAsync();
        }

        /// <summary>
        ///     Reset user settings for the selected HUD to default.
        /// </summary>
        private void BtnReset_OnClick(object sender, RoutedEventArgs e)
        {
            // Ask the user if they want to reset before doing so.
            if (ShowMessageBox(MessageBoxImage.Question, Properties.Resources.info_hud_reset,
                MessageBoxButton.YesNo) != MessageBoxResult.Yes) return;

            Logger.Info("Start resetting settings.");
            var selection = Json.SelectedHUD;
            selection.ResetAll();
            selection.Settings.SaveSettings();
            selection.ApplyCustomizations();
            selection.DirtyControls.Clear();
            LblStatus.Content = string.Format(Properties.Resources.status_reset, selection.Name, DateTime.Now);
            Logger.Info("Done resetting settings.");
        }

        /// <summary>
        ///     Return to the HUD selection page.
        /// </summary>
        private void BtnSwitch_OnClick(object sender, RoutedEventArgs e)
        {
            Logger.Info("Changing page view to: main menu.");
            EditorContainer.Children.Clear();
            Json.HighlightedHUD = null;
            Json.SelectedHUD = null;
        }

        private void BtnSetDirectory_OnClick(object sender, RoutedEventArgs e)
        {
            Logger.Info("Attempting to change the 'tf/custom' directory.");
            var previousPath = HudPath;
            SetupDirectory(true);

            if (HudPath.Equals(previousPath) || !Utilities.CheckUserPath(HudPath))
                ShowMessageBox(MessageBoxImage.Error, Properties.Resources.info_path_invalid);
        }

        /// <summary>
        ///     Opens the issue tracker for the editor.
        /// </summary>
        private void BtnReportIssue_OnClick(object sender, RoutedEventArgs e)
        {
            Utilities.OpenWebpage(Settings.Default.app_tracker);
        }

        /// <summary>
        ///     Opens the project documentation site.
        /// </summary>
        private void BtnDocumentation_OnClick(object sender, RoutedEventArgs e)
        {
            Utilities.OpenWebpage(Settings.Default.app_docs);
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
                    MessageBox.Show(Properties.Resources.info_hud_update_none,
                        Utilities.GetLocalizedString(Properties.Resources.header_update_none), MessageBoxButton.OK,
                        MessageBoxImage.Information);
                    return;
                }

                if (MessageBox.Show(Properties.Resources.info_hud_update,
                    Utilities.GetLocalizedString(Properties.Resources.header_restart_required),
                    MessageBoxButton.YesNo,
                    MessageBoxImage.Information) != MessageBoxResult.Yes) return;

                Json.Update(true);
                Debug.WriteLine(Assembly.GetExecutingAssembly().Location);
                Process.Start(Assembly.GetExecutingAssembly().Location.Replace(".dll", ".exe"));
                Environment.Exit(0);
            });
        }

        private void BtnGitHub_OnClick(object sender, RoutedEventArgs e)
        {
            Utilities.OpenWebpage(Json.HighlightedHUD.GitHubUrl);
        }

        private void BtnHuds_OnClick(object sender, RoutedEventArgs e)
        {
            Utilities.OpenWebpage(Json.HighlightedHUD.HudsTfUrl);
        }

        private void BtnDiscord_OnClick(object sender, RoutedEventArgs e)
        {
            Utilities.OpenWebpage(Json.HighlightedHUD.DiscordUrl);
        }

        private void BtnSteam_OnClick(object sender, RoutedEventArgs e)
        {
            Utilities.OpenWebpage(Json.HighlightedHUD.SteamUrl);
        }


        /// <summary>
        ///     Save the selected HUD then update the page view and controls.
        /// </summary>
        private void gridSelectHud_SelectionChanged(object sender, HUD hud)
        {
        }

        private void BtnLocalize_OnClick(object sender, RoutedEventArgs e)
        {
            if (btnLocalizeFR.IsChecked == true)
                LocalizeDictionary.Instance.Culture = new CultureInfo("fr-FR");
            else if (btnLocalizeRU.IsChecked == true)
                LocalizeDictionary.Instance.Culture = new CultureInfo("ru-RU");
            else
                LocalizeDictionary.Instance.Culture = new CultureInfo("en-US");

            // Save language preference to user settings.
            Settings.Default.user_language = LocalizeDictionary.Instance.Culture.ToString();
            Settings.Default.Save();
        }

        #endregion
    }
}