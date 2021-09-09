﻿using System;
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
using System.Windows.Media;
using System.Windows.Media.Imaging;
using AutoUpdaterDotNET;
using HUDEditor.Classes;
using HUDEditor.Properties;
using log4net;
using log4net.Config;
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
        public static string HudPath = Settings.Default.hud_directory;
        public static readonly ILog Logger = LogManager.GetLogger(MethodBase.GetCurrentMethod()?.DeclaringType);
        private readonly List<(HUD, Border)> HudThumbnails = new();
        private bool DisplayUniqueHUDsOnly;

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
            DataContext = this;

            foreach (var hud in Json.HUDList)
                AddHUDToGridView(hud);

            // Setup the user interface.
            SetPageView(Json.SelectedHUD);
            SelectionChanged(Json, Json.GetHUDByName(Settings.Default.hud_selected));
            SetupDirectory();
            Json.SelectionChanged += SelectionChanged;

            // Check for app updates.
            Logger.Info("Checking for app updates.");
            AutoUpdater.Start(Settings.Default.app_update);
        }

        public static Json Json { get; set; }

        /// <summary>
        ///     Setup the tf/custom directory, if it's not already set.
        /// </summary>
        /// <param name="userSet">Flags the process as being user initiated, skip right to the folder browser.</param>
        private void SetupDirectory(bool userSet = false)
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
                    if (browser.ShowDialog() == System.Windows.Forms.DialogResult.OK)
                    {
                        if (browser.SelectedPath.EndsWith("tf\\custom"))
                        {
                            Settings.Default.hud_directory = browser.SelectedPath;
                            Settings.Default.Save();
                            HudPath = Settings.Default.hud_directory;
                            Logger.Info("tf/custom directory is set to: " + Settings.Default.hud_directory);
                        }
                        else
                        {
                            ShowMessageBox(MessageBoxImage.Error, Properties.Resources.info_path_invalid);
                        }
                    }
                    else
                    {
                        break;
                    }
            }

            // Check one more time if a valid directory has been set.
            if (Utilities.CheckUserPath(HudPath)) return;
            Logger.Info("tf/custom directory still not set. Exiting...");
            ShowMessageBox(MessageBoxImage.Warning, Utilities.GetLocalizedString("error_app_directory"));
            Application.Current.Shutdown();
        }

        /// <summary>
        ///     Add a HUD to the list of available HUDs.
        /// </summary>
        /// <param name="hud">HUd object to add to the list.</param>
        private void AddHUDToGridView(HUD hud)
        {
            Logger.Info($"Loading {hud.Name}");
            var border = new Border
            {
                Style = (Style)Application.Current.Resources["HudListBorder"]
            };

            border.MouseEnter += (_, _) => border.BorderBrush = Brushes.Black;
            border.MouseLeave += (_, _) => border.BorderBrush = Brushes.LightGray;
            border.MouseUp += (_, _) =>
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

            var thumbnailIcon = new Label
            {
                Content = hud.Unique ? "B" : "",
                Style = (Style)Application.Current.Resources["HudListIcon"]
            };

            var thumbnailImage = new Image
            {
                Source = hud.Thumbnail != null ? new BitmapImage(new Uri(hud.Thumbnail)) : null,
                Style = (Style)Application.Current.Resources["HudListImage"]
            };

            // Prevents image from looking grainy when resized
            RenderOptions.SetBitmapScalingMode(thumbnailImage, BitmapScalingMode.Fant);

            // Build the HUD container.
            var hudIconContainer = new Grid();
            hudIconContainer.RowDefinitions.Add(new RowDefinition());
            hudIconContainer.RowDefinitions.Add(new RowDefinition());
            hudIconContainer.Children.Add(thumbnailImage);
            hudIconContainer.Children.Add(thumbnailIcon);
            hudIconContainer.Children.Add(new Label
                { Content = hud.Name, Style = (Style)Application.Current.Resources["HudListLabel"] });
            border.Child = hudIconContainer;
            GridSelectHud.Children.Add(border);
            HudThumbnails.Add((hud, border));
        }

        /// <summary>
        ///     Called when a new HUD has been selected from the list.
        /// </summary>
        /// <param name="hud">Selected HUD object.</param>
        private void SelectionChanged(object sender, HUD hud)
        {
            if (hud != null)
            {
                // Save the selected HUD.
                HudSelection = hud.Name;
                if (Json.SelectedHUDInstalled)
                    LblStatus.Content = string.Format(Properties.Resources.status_installed, Json.SelectedHUD.Name);
                else if (Directory.Exists(HudPath))
                    LblStatus.Content = string.Format(Properties.Resources.status_installed_not, Json.SelectedHUD.Name);
                else
                    LblStatus.Content = Properties.Resources.status_pathNotSet;

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
        public static MessageBoxResult ShowMessageBox(MessageBoxImage type, string message, MessageBoxButton buttons = MessageBoxButton.OK)
        {
            switch (type)
            {
                case MessageBoxImage.Error:
                    Logger.Error(message);
                    break;
                case MessageBoxImage.Warning:
                    Logger.Warn(message);
                    break;
            }

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
            catch (Exception e)
            {
                ShowMessageBox(MessageBoxImage.Error, e.Message);
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

        /// <summary>
        ///     Check if the selected hud is valid.
        /// </summary>
        /// <returns>True if the selected hud is valid.</returns>
        public static bool CheckHudSelection()
        {
            Json.SelectedHUD = Json.HighlightedHUD;
            Settings.Default.hud_selected = HudSelection = Json.SelectedHUD.Name;
            if (string.IsNullOrWhiteSpace(HudSelection))
            {
                return false;
            }
            return true;
        }

        #endregion

        #region CLICK_EVENTS

        private void TbSearchBox_TextChanged(object sender, RoutedEventArgs e)
        {
            foreach (var (hud, border) in HudThumbnails)
            {
                var searches = new[]
                {
                    hud.Name,
                    hud.Author,
                    hud.Description
                };

                var index = 0;
                var visibility = Visibility.Collapsed;
                while (visibility == Visibility.Collapsed && index < searches.Length)
                {
                    if (searches[index] != null && searches[index].ToLower().Contains(SearchBox.Text.ToLower()) && (!DisplayUniqueHUDsOnly || hud.Unique))
                        visibility = Visibility.Visible;
                    index++;
                }

                border.Visibility = visibility;
            }
        }

        private void UniqueHUDsButton_OnClick(object sender, RoutedEventArgs e)
        {
            DisplayUniqueHUDsOnly = !DisplayUniqueHUDsOnly;
            if (DisplayUniqueHUDsOnly)
            {
                UniqueHUDsButton.Foreground = Brushes.SkyBlue;
                foreach (var (hud, border) in HudThumbnails)
                    border.Visibility = hud.Unique ? Visibility.Visible : Visibility.Collapsed;
            }
            else
            {
                UniqueHUDsButton.Foreground = Brushes.White;
                foreach (var (hud, border) in HudThumbnails)
                    border.Visibility = Visibility.Visible;
            }

            TbSearchBox_TextChanged(sender, e);
        }

        /// <summary>
        ///     Invoke HUD installation or setting the tf/custom directory, if not already set.
        /// </summary>
        private void BtnInstall_OnClick(object sender, RoutedEventArgs args)
        {
            try
            {
                // Prevent switching HUD while installing to ensure that HighlightedHUD is the same as SelectedHUD at worker.RunWorkerCompleted
                BtnSwitch.IsEnabled = false;
                Json.SelectedHUD = Json.HighlightedHUD;
                Settings.Default.hud_selected = HudSelection = Json.SelectedHUD.Name;

                // Force the user to set a directory before installing.
                if (!Utilities.CheckUserPath(HudPath))
                    SetupDirectory(true);

                // Stop the process if Team Fortress 2 is still running.
                if (Utilities.CheckIsGameRunning()) return;

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
            catch (Exception e)
            {
                ShowMessageBox(MessageBoxImage.Error, $"{string.Format(Utilities.GetLocalizedString("error_hud_install"), HudSelection)} {e.Message}");
            }
        }

        /// <summary>
        ///     Invoke HUD deletion from the tf/custom directory.
        /// </summary>
        private void BtnUninstall_OnClick(object sender, RoutedEventArgs args)
        {
            try
            {
                // Check if HUDSelection is valid.
                if (!CheckHudSelection())
                {
                    BtnUninstall.IsEnabled = false;
                    return;
                }

                // Check if the HUD is installed in a valid directory.
                if (!CheckHudInstallation()) return;

                // Stop the process if Team Fortress 2 is still running.
                if (Utilities.CheckIsGameRunning()) return;

                // Remove the HUD from the tf/custom directory.
                Logger.Info($"Start uninstalling {HudSelection}.");
                Logger.Info($"Removing {HudSelection} from: {HudPath}");
                Directory.Delete(HudPath + $"\\{HudSelection}\\", true);
                Json.OnPropertyChanged("HighlightedHUD");
                Json.OnPropertyChanged("HighlightedHUDInstalled");
                LblStatus.Content = string.Format(Properties.Resources.status_uninstalled_now,
                        Settings.Default.hud_selected, DateTime.Now);
            }
            catch (Exception e)
            {
                ShowMessageBox(MessageBoxImage.Error, $"{string.Format(Utilities.GetLocalizedString("error_hud_uninstall"), HudSelection)} {e.Message}");
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
                var message = hudObject.DirtyControls.Aggregate(Properties.Resources.info_game_restart, (current, control) => current + $"\n - {control}");
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
                LblStatus.Content = string.Format(Properties.Resources.status_applied, hudObject.Name, DateTime.Now);
            };
            worker.RunWorkerAsync();
        }

        /// <summary>
        ///     Reset user settings for the selected HUD to default.
        /// </summary>
        private void BtnReset_OnClick(object sender, RoutedEventArgs e)
        {
            // Ask the user if they want to reset before doing so.
            if (ShowMessageBox(MessageBoxImage.Question, Properties.Resources.info_hud_reset, MessageBoxButton.YesNo) != MessageBoxResult.Yes) return;
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
            SetupDirectory(true);
        }

        /// <summary>
        ///     Add a HUD from folder to the shared HUDs list.
        /// </summary>
        private void BtnAddSharedHUD_OnClick(object sender, RoutedEventArgs e)
        {
            try
            {
                if (ShowMessageBox(MessageBoxImage.Information, Properties.Resources.info_add_hud, MessageBoxButton.YesNoCancel) != MessageBoxResult.Yes) return;

                var browser = new FolderBrowserDialog
                {
                    SelectedPath = $@"{HudPath}\"
                };
                if (browser.ShowDialog() != System.Windows.Forms.DialogResult.OK) return;

                AddHUDToGridView(Json.Add(browser.SelectedPath));
            }
            catch (Exception error)
            {
                ShowMessageBox(MessageBoxImage.Error, error.Message);
            }
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
                    ShowMessageBox(MessageBoxImage.Information, Properties.Resources.info_hud_update_none);
                    return;
                }

                if (ShowMessageBox(MessageBoxImage.Information, Properties.Resources.info_hud_update, MessageBoxButton.YesNo) != MessageBoxResult.Yes) return;
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
        ///     Updates localization to the selected language.
        /// </summary>
        private void BtnLocalize_OnClick(object sender, RoutedEventArgs e)
        {
            if (btnLocalizeEn.IsChecked == true)
                LocalizeDictionary.Instance.Culture = new CultureInfo("en-US");
            else if (btnLocalizeFr.IsChecked == true)
                LocalizeDictionary.Instance.Culture = new CultureInfo("fr-FR");
            else if (btnLocalizeRu.IsChecked == true)
                LocalizeDictionary.Instance.Culture = new CultureInfo("ru-RU");

            // Save language preference to user settings.
            Settings.Default.user_language = LocalizeDictionary.Instance.Culture.ToString();
            Settings.Default.Save();
        }


        private void BtnCustomize_OnClick(object sender, RoutedEventArgs e)
        {
            if (Json.HighlightedHUD is null) return;
            EditorContainer.Children.Clear();
            Json.SelectedHUD = Json.HighlightedHUD;
            Settings.Default.hud_selected = Json.SelectedHUD.Name;
            Settings.Default.Save();
            SetPageView(Json.GetHUDByName(Settings.Default.hud_selected));
        }

        #endregion
    }
}
